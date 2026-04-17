using UnityEngine;
using UnityEngine.InputSystem;

public class Mousefollower : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }
}
