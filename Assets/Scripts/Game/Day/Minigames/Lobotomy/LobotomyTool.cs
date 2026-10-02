using System.Collections.Generic;
using UnityEngine;

public class LobotomyTool : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [Header("Minigame")]
    public static ConnectWires Instance;
    public Camera cam;

    public Vector3 mousePos;
    public bool isDragging = false;
    public int score;
    public Texture2D texture; //this is what the player draws onto.

    [Header("Difficulty Modifiers")]
    public float sensitivity; //how affects how fast you can drag without hurting their eye (measured in units traveled in the last 1 second.)
    public float blinkTime; //time between blinks

    [Header("Path")]
    public List<Transform> points; //the points it the player's line to cross through to be considered successful 

    [Header("Cursor UI")]
    public RectTransform cursorUI; private Animator toolAnim;
    public RectTransform canvasRect;

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
