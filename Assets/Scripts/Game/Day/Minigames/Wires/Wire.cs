using UnityEngine;

public class Wire : MonoBehaviour
{
    public ConnectWires connectWires;
    public int index;
    public bool isStart = true;
    private LineRenderer lineRenderer;
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        DrawLine();
    }

    void DrawLine()
    {
        if (!isStart)
        {
            return;
        }
        lineRenderer.SetPosition(1, connectWires.mousePos);
    }

    public void OnCursorEnter()
    {
        if (!connectWires.isDragging)
        {
            return;
        }

        connectWires.wireHovering = this;
    }

    public void OnCursorExit()
    {
        if (!connectWires.isDragging)
        {
            return;
        }

        connectWires.wireHovering = null;
    }
}
