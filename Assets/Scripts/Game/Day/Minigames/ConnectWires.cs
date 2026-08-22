using UnityEngine;
using UnityEngine.InputSystem;

public class ConnectWires : MinigameBase
{

    [Header("Track Following")]
    public static ConnectWires Instance;
    public LayerMask trackLayer;
    public LayerMask trackEnd;
    public Camera cam;
    public float offTrackGraceTime = 0.15f;

    bool tracking = false;
    bool hasEntered = false;
    float offTrackTimer = 0f;
    bool hasPenalized = false;

    [Header("Cursor UI")]
    public RectTransform cursorUI; private Animator handAnim;
    public RectTransform canvasRect;

    private int score;

    void Awake()
    {
        Instance = this;

        handAnim = cursorUI.GetComponent<Animator>();

        Physics2D.queriesHitTriggers = true; // this is some bullshit bro

        gameObject.SetActive(false);
    }

    public override void Open(PatientInteractionInfo info)
    {
        base.Open(info);
        tracking = true;
        hasEntered = false;
        offTrackTimer = 0f;
        score = 6;
    }

    void Update()
    {
        if (cursorUI == null || canvasRect == null || cam == null) 
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, Mouse.current.position.ReadValue(), cam, out Vector3 mouseWorld))
        {
            return;
        }

        cursorUI.position = mouseWorld;

        if (!tracking) 
        {
            return;
        }

        bool onTrackNow = Physics2D.OverlapPoint(mouseWorld, trackLayer) != null;

        if (!hasEntered)
        {
            //Wait until they enter
            if (onTrackNow)
            {
                hasEntered = true;
                offTrackTimer = 0f;
            }
            return;
        }

        if (onTrackNow)
        {
            offTrackTimer = 0f;
            hasPenalized = false;
        }
        else
        {
            offTrackTimer += Time.deltaTime;
            if (offTrackTimer >= offTrackGraceTime && !hasPenalized)
            {
                OnLeftTrack();
                hasPenalized = true;
            }
        }

    if (Physics2D.OverlapPoint(mouseWorld, trackEnd) != null)
        {
            OnEndTrack();
        }

    }

    public void OnLeftTrack() // when you suck at the game
    {
        score -= 1;
        handAnim.SetTrigger("hurt");

        if (score <= 0)
        {
            OnWireResult(0); // Mega lose
            return;
        }

        offTrackTimer = 0f;
    }

    public void OnEndTrack() // when you get to the end
    {
        OnWireResult(score);
    }

    public void OnWireResult(int result)
    {
        tracking = false;
        Finish(result);
    }
}