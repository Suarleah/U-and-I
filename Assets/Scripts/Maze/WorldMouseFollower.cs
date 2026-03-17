using UnityEngine;
using UnityEngine.InputSystem;

//script for rigidbodies which follow the mouse
public class WorldMouseFollower : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0);
    }
}
