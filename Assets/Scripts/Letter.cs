using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Letter : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    private Animator animator;
    public bool checkersSolved;
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (checkersSolved)
        {
            animator.SetTrigger("unlocked");
        } else
        {
            animator.SetTrigger("locked");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }
}
