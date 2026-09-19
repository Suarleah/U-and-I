using UnityEngine;

public class Wire : MonoBehaviour
{
    public ConnectWires connectWires;
    public Color myColor; // Not racist I swear!!
    public bool isStart = true;
    public bool isConnected = false;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        lineRenderer.SetPosition(0, Vector3.zero); // (0,0,0) = wire is at object center because LOCAL
        lineRenderer.SetPosition(1, Vector3.zero); //both 0,0,0 = not visible
    }

    void Update()
    {
        if (connectWires.draggingWire == this) // only follow the mouse if this specific wire is the one being dragged
        {
            Vector3 localMouse = transform.InverseTransformPoint(connectWires.mousePos); // convert world mousePos into this wire's local space
            lineRenderer.SetPosition(1, localMouse); // stretch the line to the cursor
        }
    }

    public void OnCursorEnter() // hook to EventTrigger PointerEnter
    {
        connectWires.wireHovering = this; // tell them this wire is being hovered
    }

    public void OnCursorExit() // hook to EventTrigger PointerExit
    {
        if (connectWires.wireHovering == this) // only clear it if we're still the one being tracked
        {
            connectWires.wireHovering = null;
        }

    }

    public void OnPointerDownWire() // OnPointerDown
    {
        connectWires.BeginDrag(this); // try to start a drag from this wire
    }

    public void OnPointerUpWire() // PointerUp
    {
        connectWires.EndDrag();
    }

    public void Connect(Vector3 endPosition) // endPosition is passed in as a world-space position of my sister
    {
        isConnected = true; // lock this wire so it can't be drugged/matched again

        Vector3 localEnd = transform.InverseTransformPoint(endPosition); // convert to local space before applying
        // where has this method been my entire life

        lineRenderer.SetPosition(1, localEnd); // snap the line onto the matched wire's position
    }

    public void ResetLine()
    {
        lineRenderer.SetPosition(1, Vector3.zero); // pull the line back to sit on the wire itself (so it not visible)
    }
}