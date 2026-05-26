using UnityEngine;

public class NetworkCursor : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.GetComponent<FishNet.Object.NetworkObject>().IsOwner)
        {
            //if you dont own it, its a bit transparent
            gameObject.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 0.5f) ;
        } else
        {
            //if you own it, move it
            Vector3 screenPoint = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            screenPoint.z = 10f;
            transform.position = UnityEngine.Camera.main.ScreenToWorldPoint(screenPoint);
        }
        
    }
}
