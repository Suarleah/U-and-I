using UnityEngine;
using UnityEngine.EventSystems;

//script which the UI uses to track a rigidbody object.
public class RigidBodyTracker : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler 
{
    private Vector2 offsetRefPos;
    private bool dragged = false;
    [SerializeField] private GameObject tracked;

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = tracked.transform.position;
        Vector3 size = tracked.transform.localScale;

        if (dragged) //if being dragged, move with the mouse
        {
            gameObject.transform.position = Camera.main.WorldToScreenPoint(pos);
        } else //else move with the window
        {
            tracked.transform.position = Camera.main.ScreenToWorldPoint(gameObject.transform.position);
            tracked.transform.position = new Vector3(tracked.transform.position.x, tracked.transform.position.y, 0);//keeps in camera
        }
        gameObject.transform.localScale = CanvasWorldConversion.wtcSize(size, Camera.main, GetComponentInParent<Canvas>());
    }




    [SerializeField] private Joint2D mouseJoint;


    //used to check if mouse is currently dragging
    public void OnPointerDown(PointerEventData eventData)
    {
        mouseJoint.connectedBody = tracked.GetComponent<Rigidbody2D>();
        dragged = true;
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("dragging");
        mouseJoint.connectedBody = tracked.GetComponent<Rigidbody2D>();
        dragged = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        mouseJoint.connectedBody = null;
        dragged = false;
    }
}
