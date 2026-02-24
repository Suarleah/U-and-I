using UnityEngine;
using UnityEngine.EventSystems;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    private Vector2 offsetRefPos;

    public void OnPointerDown(PointerEventData eventData)
    {
        offsetRefPos = new Vector2(transform.position.x - eventData.position.x, 
        transform.position.y - eventData.position.y);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position + offsetRefPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
    }
}