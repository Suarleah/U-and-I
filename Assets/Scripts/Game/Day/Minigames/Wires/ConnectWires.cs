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
    public Wire wireHovering;   // whatever wire the cursor is currently over
    public Wire draggingWire;   // the actual start wire the player picked up
    public int wireCount = 4;   // difficulty

    [Header("Wire Spawn")]
    public Transform start;
    public Transform end;
    public GameObject wirePrefab;
    public Color[] wireColors;

    [Header("Cursor UI")]
    public RectTransform cursorUI; private Animator handAnim;
    public RectTransform canvasRect;

    private int correctConnections;

    void Awake()
    {
        Instance = this; // set the singleton so other scripts can grab this easily

        handAnim = cursorUI.GetComponent<Animator>(); // grab the animator off the cursor UI

        gameObject.SetActive(false); // start off disabled until the minigame is opened
    }

    public override void Open(PatientInteractionInfo info)
    {
        base.Open(info);
        correctConnections = 0; // reset progress every time the minigame opens
        SpawnWires();
    }

    private void SpawnWires()
    {
        List<Color> colorPool = wireColors.ToList();
        // Temporary list of all potential colors copied from a set array when spawning wires

        for (int i = 0; i < wireCount; i++) // wireCount is a public int that should be changed by the difficulty (default 4)
        {
            int x = Random.Range(0, colorPool.Count); // get a random color

            GameObject s = Instantiate(wirePrefab, start);
            GameObject e = Instantiate(wirePrefab, end);
            // Instantiate 2 copies of the wire object, one as a child of the start area and the other of the end area

            Wire sWire = s.GetComponent<Wire>(); // Wire component off the start wire
            Wire eWire = e.GetComponent<Wire>(); // c Wire component off the end wire

            s.GetComponent<Image>().color = colorPool[x];
            e.GetComponent<Image>().color = colorPool[x];
            // Set the color of both wires to the SAME random color chosen earlier

            LineRenderer sLine = s.GetComponent<LineRenderer>(); // grab the start wire's line renderer
            LineRenderer eLine = e.GetComponent<LineRenderer>(); // grab the end wire's line renderer
            sLine.startColor = colorPool[x]; sLine.endColor = colorPool[x];
            eLine.startColor = colorPool[x]; eLine.endColor = colorPool[x];
            // Set the line (that shows up between cursor and wire when dragging) to be the same color as the wire itself

            sWire.colorIndex = x; // store the color index on the start wire so we can match it later
            eWire.colorIndex = x; // store the same color index on the end wire
            sWire.connectWires = this; // give the wire a reference back to this manager
            eWire.connectWires = this; // same for the end wire

            eWire.isStart = false;
            // Set the wire that was spawned in the end area to not be a starting wire (because it is an ending wire)

            colorPool.RemoveAt(x);
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
            return; // if mouse not on screen
        }

        mousePos = mouseWorld; // save the mouse's world position for other scripts to use
        cursorUI.position = mousePos; // move the cursor UI to follow the mouse

    }

    public void BeginDrag(Wire w) // call this from a start wire's PointerDown
    {
        if (w == null || !w.isStart || w.isConnected) return; // ignore clicks that aren't a valid, unconnected start wire

        isDragging = true; // flag that a drag has started
        draggingWire = w; // remember which wire is being dragged
    }

    private void EndDrag() // onpointerup
    {
        if (!isDragging)
        {
            return;
        }


        isDragging = false; // drag is over either way

        if (draggingWire != null && wireHovering != null
            && !wireHovering.isStart && !wireHovering.isConnected
            && wireHovering.colorIndex == draggingWire.colorIndex) // check we dropped on the MATCHINNG end wire
        {
            draggingWire.Connect(wireHovering.transform.position); // lock the start wire's line onto the end wire
            wireHovering.Connect(wireHovering.transform.position); // mark the end wire as connected too


            correctConnections++; // count this as a correct match

            if (correctConnections >= wireCount) // check if every wire has been matched
            {
                OnGameResult(1); // win the minigame
            }
        }
        else
        {
            draggingWire?.ResetLine(); // snap the line back if it wasn't dropped on the right wire
        }

        draggingWire = null; // clear the dragging reference either way
    }

    public void OnGameResult(int result)
    {
        Finish(result);
    }
}