using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SecurityMenu : MonoBehaviour
{
    //this is the script that manages the doors (ie which doors are on cooldown at any given time, which doors are opened/closed for the janitor)
   
    public List<Door> doors; //keeps track of doors on the map
    public List<CleaningSection> cleaningZones;

    public List<Button> doorButtons; //corresponding buttons for doors (2 for each door 1 is open 1 is close)
    public List<Button> zoneButtons; //corresponding buttons for zones


    
    private void Update()
    {
        for (int i = 0; i < doors.Count; i++)
        {
            if (!doors[i].onCD.Value) //right now the button activation is based on serverside (but in future id like to make it predictive, just too much work right now)
            {
                //first button is open, second is close
                if (doors[i].closed.Value)
                {
                    doorButtons[i*2].interactable = (true); 
                    doorButtons[i*2 + 1].interactable = (false);
                } else
                {
                    doorButtons[i*2].interactable = (false); 
                    doorButtons[i*2 + 1].interactable = (true);
                }
                
                
            } else
            {
                //set open and close buttons to false
                doorButtons[i*2].interactable = (false); 
                doorButtons[i*2 + 1].interactable = (false);
            }
        }

        for (int i = 0; i < cleaningZones.Count; i++)
        {
            if (cleaningZones[i].activatable.Value)
            {
                //set open and close buttons to true
                zoneButtons[i].interactable = (true); 
            }
        }
    }

    public void CleanZone(int section)
    {
        cleaningZones[section].CleanUp();
    }

    public void OpenDoor(int door)
    {
        doors[door].OpenDoor();
    }

     public void CloseDoor(int door)
    {
        doors[door].CloseDoor();
    }

}
