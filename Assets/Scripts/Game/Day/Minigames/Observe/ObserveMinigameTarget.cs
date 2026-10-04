using System.Collections.Generic;
using UnityEngine;

//the thing you are supposed to be observing
[RequireComponent(typeof(RectTransform))]
public class ObserveMinigameTarget : MonoBehaviour
{
    public static readonly List<ObserveMinigameTarget> All = new List<ObserveMinigameTarget>(); // every active target, checked by ObserveMinigame

    public bool IsLit { get; private set; } // true while the spotlight is over this
    private RectTransform rect;

    private Vector2 destination; 
    private float wanderTimer;   

    void Awake()
    {
        rect = (RectTransform)transform;
    }

    void OnEnable()
    {
        All.Add(this);
        wanderTimer = 0f; // pick a destination on the first Update
    }

    void OnDisable()
    {
        All.Remove(this);
        SetLit(false);
    }

        void Update()
    {
        var game = ObserveMinigame.Instance;
        if (game == null || game.wanderArea == null) return;

        // time to pick a new spot
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            PickNewDestination(game);
        }

        // move toward the destination (in wanderArea space so speed is in canvas units)
        RectTransform area = game.wanderArea;
        Vector3 local = area.InverseTransformPoint(transform.position);
        Vector2 next = Vector2.MoveTowards(local, destination, game.patientSpeed * Time.deltaTime);
        transform.position = area.TransformPoint(new Vector3(next.x, next.y, local.z));

        if (IsLit)
        {
            game.AddScore(game.scorePerSecond * Time.deltaTime);
        }
    }

    void PickNewDestination(ObserveMinigame game)
    {
        RectTransform area = game.wanderArea;

        // half this target's size in wanderArea units, so it stays fully inside the area
        Vector2 half = Vector2.Scale(rect.rect.size, rect.lossyScale) / area.lossyScale.x * 0.5f;
        Rect r = area.rect;

        float minX = r.xMin + half.x, maxX = r.xMax - half.x;
        float minY = r.yMin + half.y, maxY = r.yMax - half.y;
        if (minX > maxX) minX = maxX = r.center.x; // target bigger than area, just center it
        if (minY > maxY) minY = maxY = r.center.y;

        destination = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        wanderTimer = Random.Range(game.wanderInterval.x, game.wanderInterval.y);
    }

    // circle (spotlight) vs rect (this target). done in this target's local space so rotation/scale are handled
    public bool OverlapsCircle(Vector3 centerWorld, float radiusWorld)
    {
        Vector2 c = rect.InverseTransformPoint(centerWorld);
        float r = radiusWorld / rect.lossyScale.x;
        Rect rr = rect.rect;

        // closest point on the rect to the circle center
        Vector2 closest = new Vector2(Mathf.Clamp(c.x, rr.xMin, rr.xMax), Mathf.Clamp(c.y, rr.yMin, rr.yMax));
        return (closest - c).sqrMagnitude <= r * r;

        // for "spotlight must cover the center" instead, use:
        // return (rr.center - c).sqrMagnitude <= r * r;
    }

    public void SetLit(bool lit)
    {
        if (lit == IsLit) return;
        IsLit = lit;

        if (lit) OnSpotlightEnter();
        else OnSpotlightExit();
    }

    void OnSpotlightEnter()
    {
        Debug.Log("enter");
    }

    void OnSpotlightExit()
    {
        Debug.Log("exit");
    }
}