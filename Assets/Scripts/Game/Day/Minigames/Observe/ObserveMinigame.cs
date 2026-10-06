using System.Collections.Generic;
using GameKit.Dependencies.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObserveMinigame : MinigameBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [Header("Minigame")]
    public static ObserveMinigame Instance;
    public Camera cam;

    public Vector3 mousePos;
    public int score; //the final score

    
   
    public float timer = 0f;

    [Header("Difficulty Modifiers")]
    public float spotlightSize;
    public float spotlightFollowSpeed = 6f; // higher = faster
    public float patientSpeed = 200f; // how fast targets wander, in canvas units per second
    public Vector2 wanderInterval = new Vector2(1f, 3f); // min/max seconds before a target picks a new spot
    
    //the score points under here are "fake score". basically once it reaches a final amount, itll end the minigame, but your score is based on your time
    public float scorePerSecond = 1f; //how mcuh score the target gives per second 
    [SerializeField] private float fakescore = 0;


    [Header("Wander")]
    public RectTransform wanderArea; // observetargets stay inside this. leave empty to use the whole canvas

    [Header("Spotlight")]
    public RectTransform cursorUI; 
    private Animator spotlightAnim;
    public RectTransform canvasRect;

     private bool snapSpotlight;        // snap to the mouse on the first frame after opening

    void Awake()
    {
        Instance = this; // set the singleton so other scripts can grab this easily
        if (wanderArea == null) wanderArea = canvasRect; // default to the whole canvas

        spotlightAnim = cursorUI.GetComponent<Animator>(); // grab the animator off the cursor UI
        cursorUI.localScale = new Vector3(spotlightSize, spotlightSize, spotlightSize);
        timer = 0f;

        
        //gameObject.SetActive(false); // start off disabled until the minigame is opened
    }

    public override void Open()
    {
        base.Open();

        score = 0;
        fakescore = 0f;
        snapSpotlight = true; // so the spotlight doesn't fly in from wherever it was last time
    }

        void Update()
    {
        timer += Time.deltaTime;
        if (cursorUI == null || canvasRect == null || cam == null) 
        {
            return;
        }

        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, Mouse.current.position.ReadValue(), cam, out Vector3 mouseWorld))
        {
            SetAllLit(false); // mouse off screen = spotlight isn't on anything
            return; // if mouse not on screen
        }

        mousePos = mouseWorld; // save the mouse's world position for other scripts to use

        if (snapSpotlight)
        {
            cursorUI.position = mousePos;
            snapSpotlight = false;
        }
        else
        {
            // follows cursor inexactly instead
            float t = 1f - Mathf.Exp(-spotlightFollowSpeed * Time.deltaTime);
            cursorUI.position = Vector3.Lerp(cursorUI.position, mousePos, t);
        }

        CheckSpotlight();
    }

    // checks every target against the spotlight circle
    void CheckSpotlight()
    {
        Vector3 center = cursorUI.TransformPoint(cursorUI.rect.center); // works even if the pivot isn't centered
        float radius = Mathf.Min(cursorUI.rect.width, cursorUI.rect.height) * 0.5f * spotlightSize * cursorUI.lossyScale.x; // lossyScale picks up animator scaling too

        var targets = ObserveMinigameTarget.All;
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (i >= targets.Count) continue; // a target got disabled during a callback
            targets[i].SetLit(targets[i].OverlapsCircle(center, radius));
        }
    }

    void SetAllLit(bool lit)
    {
        var targets = ObserveMinigameTarget.All;
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (i >= targets.Count) continue;
            targets[i].SetLit(lit);
        }
    }

    public void AddScore(float amount)
    {
        fakescore += amount;
        if (fakescore > 10){
            OnGameResult(5);
        }
    }

    public void OnGameResult(int result)
    {
        Finish(result);
    }
}
