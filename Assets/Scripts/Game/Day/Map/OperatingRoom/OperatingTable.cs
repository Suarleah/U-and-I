using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class OperatingTable : Interactable
{
    InputActionAsset MinigameInputs;
    public ConnectWires wiresminigame;
    public TherapyMinigame therapyminigame;

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
        localOnCD = onCD.Value;
        if (closest) 
        {
            c.gameObject.SetActive(true);
            if (Vector2.Distance(gameObject.transform.position, player.transform.position) > 5f)
            {
                closest = false;
            }

        } else
        {
            c.gameObject.SetActive(false);
            
        }

        if (Vector3.Distance(player.transform.position, transform.position) > 10f)
        {
            Close();
        }
        operationTooltipText.text = "press \"E\" to perform " + mode.ToString() + ".";
        
    }
    //theoretically, when the minigame starts, we turn off player movement.
    public override void Interact()
    {
        if (onCD.Value)
        {
            GiveFeedback("On Cooldown!");
            return;
        }


        UIManager.Instance.currentInteraction = this;
        //start the minigame, pass in the current player and the current patient so that the effects of the minigame can actually affect them
        

        if (!player.GetComponent<PlayerMovement>().followingPatient)
        {
            GiveFeedback("No patient following!!");
            //return;
        } else
        {
            PatientInteractable patientInfo = player.GetComponent<PlayerMovement>().followingPatient.interactable;
        }
        switch (mode)
        {
            case (Operation.Observe):
                break;
            case (Operation.Therapy):
                therapyminigame.Open();
                break;  
            case (Operation.Lobotomy):
                break;
            case (Operation.Electric_Chair):
                wiresminigame.Open();
                break;

        }

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
