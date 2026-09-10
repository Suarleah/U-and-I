using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ConnectWires : MinigameBase
{

    [Header("Minigame")]
    public static ConnectWires Instance;
    public Camera cam;
    
    public Vector3 mousePos;
    public bool isDragging = false;
    public Wire wireHovering;
    public int wireCount; //difficulty

    [Header("Wire Spawn")]
    public Transform start;
    public Transform end;
    // Needed to Spawn wire prefab as children
    public GameObject wirePrefab;
    public Color[] wireColors; // idk how to get a random normal color


    [Header("Cursor UI")]
    public RectTransform cursorUI; private Animator handAnim;
    public RectTransform canvasRect;

    private int score;

    void Awake()
    {
        Instance = this;

        handAnim = cursorUI.GetComponent<Animator>();

        gameObject.SetActive(false);
    }

    public override void Open(PatientInteractionInfo info)
    {
        base.Open(info);
        score = 6;
        SpawnWires();
    }

    private void SpawnWires()
    {
        List<Color> geniusCoding = wireColors.ToList();
        // Temporary list of all potential colors copied from a set array when spawning wires

        for (int i = 0; i < wireCount; i++) // wireCount is a public int that should be changed by the difficulty (defualt 4)
        {
            int x = Random.Range(0, geniusCoding.Count - 1); // get a random color

            GameObject s = Instantiate(wirePrefab, start);
            GameObject e = Instantiate(wirePrefab, end);
            // Instantiate 2 copies of the wire object, one as a child of the start area and the other of the end area

            s.GetComponent<Image>().color = geniusCoding[x];
            e.GetComponent<Image>().color = geniusCoding[x];
            // Set the color of both wires to the random color chosen earlier

            s.GetComponent<LineRenderer>().startColor = geniusCoding[x]; s.GetComponent<LineRenderer>().endColor = geniusCoding[x];
            e.GetComponent<LineRenderer>().startColor = geniusCoding[x]; e.GetComponent<LineRenderer>().endColor = geniusCoding[x];
            // Set the line (that shows up between cursor and wire when dragging) to be the same color as the wire itself

            e.GetComponent<Wire>().isStart = false;
            // Set the wire that was spawned in the end area to not be a starting wire (because it is an ending wire)

            geniusCoding.RemoveAt(x);
            // Remove the color from the temp list so it cannot be repeated this spawn cycle

        }
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

        mousePos = mouseWorld;
        cursorUI.position = mousePos;

    }

    public void CursorEnteredWireStart(int index) // Wire w
    {
        if (!isDragging)
        {
            return;
        }
    }
    public void OnDragWire()
    {
        isDragging = true;
    }
    public void OnEndDragWire()
    {
        isDragging = false;

        if (wireHovering != null)
        {
            if (wireHovering.isStart);
        }
    }

    public void OnGameResult(int result)
    {
        Finish(result);
    }
}