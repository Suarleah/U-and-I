using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObserveMinigame : MinigameBase
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [Header("Minigame")]
    public static ObserveMinigame Instance;
    public Camera cam;

    public Vector3 mousePos;
    public int score;
    public float timer = 0f;

    [Header("Difficulty Modifiers")]
    public float spotlightSize;
    public float patientSpeed;

    [Header("Spotlight")]
    public RectTransform cursorUI; 
    private Animator spotlightAnim;
    public RectTransform canvasRect;

    
    void Awake()
    {
        Instance = this; // set the singleton so other scripts can grab this easily

        spotlightAnim = cursorUI.GetComponent<Animator>(); // grab the animator off the cursor UI

        timer = 0f;
        //gameObject.SetActive(false); // start off disabled until the minigame is opened
    }

    public override void Open()
    {
        base.Open();
        
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
            return; // if mouse not on screen
        }

        mousePos = mouseWorld; // save the mouse's world position for other scripts to use
        cursorUI.position = mousePos; // move the cursor UI to follow the mouse

    }

    public void OnGameResult(int result)
    {
        Finish(result);
    }
}
