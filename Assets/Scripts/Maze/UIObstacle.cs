using UnityEngine;

//Currentl used for maze wall, UI object that can have collision
public class UIObstacle : MonoBehaviour
{
    [SerializeField] private GameObject tracked;
    void Update()
    {
        tracked.transform.position = Camera.main.ScreenToWorldPoint(gameObject.transform.position);
        tracked.transform.position = new Vector3(tracked.transform.position.x, tracked.transform.position.y, 0);
        tracked.transform.localScale = CanvasWorldConversion.ctwSize(gameObject.transform.localScale, Camera.main, GetComponentInParent<Canvas>());
    }

}
