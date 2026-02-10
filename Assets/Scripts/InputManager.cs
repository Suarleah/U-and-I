using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Only Visible for Debugging")]
    [SerializeField] private Vector2 mousePos;
    [SerializeField] private GameObject selected;
    [SerializeField] private GameObject lastSelected;
    void Start()
    {

    }

    void Update()
    {
        Vector2 fakePos = Mouse.current.position.ReadValue();
        mousePos = Camera.main.ScreenToWorldPoint(new Vector3(fakePos.x, fakePos.y, 0));
        // The mouse position is equal to the main camera converting the raw mouse data from screen pixels to world coordinates 
        
    }
}
