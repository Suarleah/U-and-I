using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobotomyMinigame : MinigameBase
{
    [Header("Minigame")]
    public static LobotomyMinigame Instance;
    public Camera cam;

    public Vector3 mousePos;
    public bool isDragging = false;
    public int score;          // starts at 0, goes up as you trace the curve
    public int maxScore = 10;  // score for tracing the whole curve

    [Header("Difficulty Modifiers")]
    public float irritationSpeed = 0.15f; // irritation per second. eye blinks at 1, so 0.15 = blink every ~6.7s
    public float maxOffCurve = 30f;       // how far (canvas units) the mouse can stray from the curve before failing
    public float twitchThreshold = 0.6f;  // irritation level where the eye starts twitching (the warning)
    public float blinkDuration = 0.75f;   // how long the eye stays shut
    public float maxSpeed = 400f;         // max canvas units the cursor can travel within speedWindow while dragging
    public float speedWindow = 1f;        // seconds of movement we look back over

    [Header("Path")]
    public Transform pointsParent;        // the eye. vector points are local to this, and its children can be used as points
    public List<Transform> points;        // used when no vector points are set. filled from pointsParent's children on Open
    public List<Vector2> vectorPoints;    // set by SetPoints(List<Vector2>), or filled by the context menu for copying into a patient
    public float finishTolerance = 5f;    // how close to the end counts as finished (canvas units)
    public float lookAhead = 100f;        // how far ahead along the curve we search. stops players from skipping across the curve

    [Header("Lines (optional)")]
    public LineRenderer curveLine;        // draws the full curve to follow (the highlight)
    public LineRenderer progressLine;     // draws the part they've already traced

    [Header("Eye")]
    public Animator eyeAnim;              // needs bool params "twitching" and "blinking"

    [Header("Cursor UI")]
    public RectTransform cursorUI; private Animator toolAnim;
    public RectTransform canvasRect;

    // path data, in canvasRect local space so all distances are canvas units (same as ObserveMinigame)
    private Vector2[] path;
    private float[] cumulative;   // distance along the path at each point
    private float totalLength;
    private float progress;       // how far along the path the player has traced
    private int currentSegment;
    private bool useVectorPoints; // true once SetPoints(List<Vector2>) is called

    private float irritation;
    private float blinkTimer;
    private bool isBlinking;
    private bool finished;

    // speed tracking
    private readonly Queue<(float time, float dist)> moveSamples = new Queue<(float time, float dist)>();
    private float recentDistance;
    private Vector2 lastMouseLocal;

    void Awake()
    {
        Instance = this; // set the singleton so other scripts can grab this easily
        toolAnim = cursorUI.GetComponent<Animator>();
    }

    public override void Open()
    {
        base.Open();

        BuildPathFromSource();

        score = 0;
        progress = 0f;
        currentSegment = 0;
        irritation = 0f;
        isBlinking = false;
        isDragging = false;
        finished = false;

        moveSamples.Clear();
        recentDistance = 0f;

        SetEye("twitching", false);
        SetEye("blinking", false);
        UpdateProgressLine();
    }

    // ---------------- Setting the curve ----------------

    // give the minigame a curve as plain vectors, local to pointsParent (the eye). no GameObjects created
    public void SetPoints(List<Vector2> localPoints)
    {
        vectorPoints = new List<Vector2>(localPoints); // copy so outside edits don't change ours
        useVectorPoints = true;
        RefreshPath();
    }

    // use a parent's direct children as the curve, in hierarchy order (top child = start of curve)
    public void SetPoints(Transform parent)
    {
        pointsParent = parent;
        useVectorPoints = false;
        RefreshPath();
    }

    // works out the curve from whichever source is active and converts it to canvas space
    private void BuildPathFromSource()
    {
        Vector2[] canvasPoints;

        if (useVectorPoints)
        {
            canvasPoints = new Vector2[vectorPoints.Count];
            for (int i = 0; i < vectorPoints.Count; i++)
            {
                Vector3 world = pointsParent != null
                    ? pointsParent.TransformPoint(vectorPoints[i])
                    : canvasRect.TransformPoint(vectorPoints[i]); // no parent = treat vectors as canvas-local
                canvasPoints[i] = canvasRect.InverseTransformPoint(world);
            }
        }
        else
        {
            if (pointsParent != null) FillPointsFromChildren(pointsParent);

            canvasPoints = new Vector2[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                canvasPoints[i] = canvasRect.InverseTransformPoint(points[i].position); // world -> canvas local
            }
        }

        BuildPath(canvasPoints);
    }

    private void FillPointsFromChildren(Transform parent)
    {
        points = new List<Transform>(parent.childCount);
        foreach (Transform child in parent)
        {
            points.Add(child);
        }
    }

    private void BuildPath(Vector2[] canvasPoints)
    {
        path = canvasPoints;
        cumulative = new float[path.Length];

        for (int i = 1; i < path.Length; i++)
        {
            cumulative[i] = cumulative[i - 1] + Vector2.Distance(path[i - 1], path[i]);
        }

        totalLength = path.Length > 0 ? cumulative[path.Length - 1] : 0f;
        DrawCurveLine();
    }

    // rebuilds the path if the minigame is already open, so swapping curves mid-game doesn't break progress tracking
    private void RefreshPath()
    {
        if (!gameObject.activeInHierarchy) return; // not open yet, Open() will build it

        BuildPathFromSource();
        progress = 0f;
        currentSegment = 0;
        score = 0;
        isDragging = false;
        UpdateProgressLine();
    }

    // authoring helper: lay the curve out as children of pointsParent, then right-click the component ->
    // "Copy Children To Vector Points". copy the resulting vectorPoints list into the patient
    [ContextMenu("Copy Children To Vector Points")]
    private void CopyChildrenToVectorPoints()
    {
        if (pointsParent == null) return;

        vectorPoints = new List<Vector2>(pointsParent.childCount);
        foreach (Transform child in pointsParent)
        {
            vectorPoints.Add(pointsParent.InverseTransformPoint(child.position));
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this); // so the list actually saves in the scene
#endif
    }

    // ---------------- Update ----------------

    void Update()
    {
        if (finished || cursorUI == null || canvasRect == null || cam == null || path == null || path.Length < 2)
        {
            return;
        }

        UpdateIrritation();
        if (finished) return; // blinking while held can fail the game

        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, Mouse.current.position.ReadValue(), cam, out Vector3 mouseWorld))
        {
            if (isDragging) Fail(); // dragged off screen = way off the curve
            return;
        }

        mousePos = mouseWorld;
        cursorUI.position = mousePos;
        Vector2 mouseLocal = canvasRect.InverseTransformPoint(mousePos);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryBeginDrag(mouseLocal);
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false; // letting go is fine, they resume from where they stopped
        }

        if (isDragging)
        {
            TraceCurve(mouseLocal);
        }
    }

    // ---------------- Irritation / blinking ----------------

    private void UpdateIrritation()
    {
        if (!isBlinking)
        {
            irritation += irritationSpeed * Time.deltaTime;
            SetEye("twitching", irritation >= twitchThreshold);
            if (irritation >= 0.5f)
            {
                Debug.Log("irritated!");
            } 
            if (irritation >= 0.8f)
            {
                Debug.Log("twitching!");
            }

            if (irritation >= 1f)
            {
                isBlinking = true;
                blinkTimer = blinkDuration;
                Debug.Log("Blink!");
                SetEye("twitching", false);
                SetEye("blinking", true);
            }
        }

        if (isBlinking)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                Fail(); // still holding while the eye closed
                return;
            }

            blinkTimer -= Time.deltaTime;
            if (blinkTimer <= 0f)
            {
                isBlinking = false;
                irritation = 0f;
                SetEye("blinking", false);
            }
        }
    }

    // ---------------- Tracing ----------------

    private void TryBeginDrag(Vector2 mouseLocal)
    {
        // must start (or resume) near the point they last reached
        Vector2 resumePoint = PointAtDistance(progress);
        if (Vector2.Distance(mouseLocal, resumePoint) <= maxOffCurve)
        {
            isDragging = true;
            ResetSpeed(mouseLocal); // so the jump from where they clicked doesn't count as movement
        }
    }

    private void TraceCurve(Vector2 mouseLocal)
    {
        if (TooFast(mouseLocal))
        {
            Fail(); // moved too fast and hurt the eye
            return;
        }

        float bestDist = float.MaxValue;
        float bestProgress = progress;
        int bestSeg = currentSegment;

        // only check segments from where they are up to lookAhead further along,
        // so they can't jump to a different part of the curve that happens to be nearby
        for (int s = currentSegment; s < path.Length - 1; s++)
        {
            if (cumulative[s] > progress + lookAhead) break;

            Vector2 a = path[s];
            Vector2 ab = path[s + 1] - a;
            float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(mouseLocal - a, ab) / ab.sqrMagnitude) : 0f;
            float d = Vector2.Distance(mouseLocal, a + ab * t);

            if (d < bestDist)
            {
                bestDist = d;
                bestProgress = cumulative[s] + ab.magnitude * t;
                bestSeg = s;
            }
        }

        if (bestDist > maxOffCurve)
        {
            Fail();
            return;
        }

        if (bestProgress > progress) // progress never goes backwards
        {
            progress = bestProgress;
            currentSegment = bestSeg;
        }

        score = Mathf.FloorToInt(progress / totalLength * maxScore);
        UpdateProgressLine();

        if (progress >= totalLength - finishTolerance)
        {
            Succeed();
        }
    }

    private void ResetSpeed(Vector2 mouseLocal)
    {
        moveSamples.Clear();
        recentDistance = 0f;
        lastMouseLocal = mouseLocal;
    }

    // adds this frame's movement and drops anything older than speedWindow
    private bool TooFast(Vector2 mouseLocal)
    {
        float now = Time.time;
        float moved = Vector2.Distance(mouseLocal, lastMouseLocal);
        lastMouseLocal = mouseLocal;

        moveSamples.Enqueue((now, moved));
        recentDistance += moved;

        while (moveSamples.Count > 0 && now - moveSamples.Peek().time > speedWindow)
        {
            recentDistance -= moveSamples.Dequeue().dist;
        }

        return recentDistance > maxSpeed;
    }

    private Vector2 PointAtDistance(float d)
    {
        for (int s = 0; s < path.Length - 1; s++)
        {
            if (cumulative[s + 1] >= d)
            {
                float segLen = cumulative[s + 1] - cumulative[s];
                float t = segLen > 0f ? (d - cumulative[s]) / segLen : 0f;
                return Vector2.Lerp(path[s], path[s + 1], t);
            }
        }
        return path[path.Length - 1];
    }

    // ---------------- Lines ----------------

    private void DrawCurveLine()
    {
        if (curveLine == null || path == null) return;

        curveLine.useWorldSpace = true;
        curveLine.positionCount = path.Length;
        for (int i = 0; i < path.Length; i++)
        {
            curveLine.SetPosition(i, canvasRect.TransformPoint(path[i]));
        }
    }

    private void UpdateProgressLine()
    {
        if (progressLine == null || path == null || path.Length < 2) return;

        progressLine.useWorldSpace = true;
        progressLine.positionCount = currentSegment + 2;
        for (int i = 0; i <= currentSegment; i++)
        {
            progressLine.SetPosition(i, canvasRect.TransformPoint(path[i]));
        }
        progressLine.SetPosition(currentSegment + 1, canvasRect.TransformPoint(PointAtDistance(progress)));
    }

    // ---------------- Helpers / results ----------------

    private void SetEye(string param, bool value)
    {
        if (eyeAnim != null) eyeAnim.SetBool(param, value);
    }

    private void Succeed()
    {
        finished = true;
        isDragging = false;
        score = maxScore;
        OnGameResult(6);
    }

    private void Fail()
    {
        finished = true;
        isDragging = false;
        float fraction = totalLength > 0f ? progress / totalLength : 0f;
        OnGameResult(1 + Mathf.FloorToInt(fraction * 5f)); // returns 1 + fraction of progress * 5
    }

    public void OnGameResult(int result)
    {
        Finish(result);
    }

    public override void Configure(Patient patient)
    {
        LobotomySettings s = patient.GetLobotomySettings();
        irritationSpeed = s.irritationSpeed;
        maxOffCurve = s.maxOffCurve;
        twitchThreshold = s.twitchThreshold;
        blinkDuration = s.blinkDuration;
        maxSpeed = s.maxSpeed;

        // Set the curve directly instead of through SetPoints, so a patient with no custom curve
        // goes back to the default instead of keeping the previous patient's curve.
        if (s.curve != null && s.curve.Count >= 2)
        {
            vectorPoints = new List<Vector2>(s.curve);
            useVectorPoints = true;
        }
        else
        {
            useVectorPoints = false;
        }
        // Open() calls BuildPathFromSource, which picks up whichever source is active.
    }
}