using UnityEngine;

public class Wire : MonoBehaviour
{
    public ConnectWires connectWires;
    public int index;
    public bool isStart = true;
    void Start()
    {
        
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
