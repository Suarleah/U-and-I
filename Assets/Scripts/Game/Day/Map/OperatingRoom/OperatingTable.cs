using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class OperatingTable : Interactable
{
    InputActionAsset MinigameInputs;

    public enum Operation
    {
        Observe = 0,
        Therapy = 1,
        Lobotomy = 2,
        Electric_Chair = 3

    }

    public TextMeshProUGUI operationTooltipText;

    public Operation mode;


    void Update()
    {
        operationTooltipText.text = "press \"E\" to perform " + mode.ToString() + ".";
    }
    //theoretically, when the minigame starts, we turn off player movement.
    public void Interact()
    {
        if (onCD.Value)
        {
            GiveFeedback("On Cooldown!");
            return;
        }

        UIManager.Instance.currentInteraction = this;

        

        StartCoroutine(goOnCooldown(cooldown));

    }

    public override void Close()
    {
        closest = false;
        interacting = false;
        
        if (UIManager.Instance.currentInteraction == this)
        {
            UIManager.Instance.currentInteraction = null;
        }
        //unfollow
        
    }
}
