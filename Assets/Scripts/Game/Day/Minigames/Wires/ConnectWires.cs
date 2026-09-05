using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ConnectWires : MinigameBase
{

    [Header("Minigame")]
    public static ConnectWires Instance;
    public Camera cam;
    public Vector3 mousePos;
    public bool isDragging = false;
    public Wire wireHovering;

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