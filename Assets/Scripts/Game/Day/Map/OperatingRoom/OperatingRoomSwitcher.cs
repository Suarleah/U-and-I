using System;
using UnityEngine;


public class OperatingRoomSwitcher : Interactable
{

    public OperatingTable table; //the operating table this one is connected to.
    public OperatingRoomFloor operatingFloor; //the floor this is connected to


    public override void UIButtonPressed(string info)
    {
        if (onCD.Value)
        {
            GiveFeedback("On Cooldown!");
            return;
        }
        //theoretically also changes the room decor and kills the player in the area.
        if (info == "Observe")
        {
            
           table.mode = OperatingTable.Operation.Observe;
           operatingFloor.Switch(table.mode);
        }

        if (info == "Therapy")
        {
            table.mode = OperatingTable.Operation.Therapy;
            operatingFloor.Switch(table.mode);
        }

        if (info == "Lobotomy")
        {
            table.mode = OperatingTable.Operation.Lobotomy;
            operatingFloor.Switch(table.mode);
        }

        if (info == "Electric Chair")
        {
           table.mode = OperatingTable.Operation.Electric_Chair;
           operatingFloor.Switch(table.mode);
        }
        StartCoroutine(goOnCooldown(cooldown));
    }

    public void Update() 
    {
        if (closest) 
        {
            c.gameObject.SetActive(true);
            if (Vector2.Distance(transform.position, player.transform.position) > 5f)
            {
                closest = false;
            }

        } else
        {
            c.gameObject.SetActive(false);
        }

    } 

    public override void Close()
    {
        closest = false;
        interacting = false;
        //unfollow
        
    }
}
