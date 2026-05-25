using UnityEngine;


//this is a class for the patient's actual ingame behavior, so movement, attacks, health, etc.
public class Patient : MonoBehaviour
{
    int room; //the room # they are staying at
    bool escaped; //whether or not they're currently escaped
    int patience; //most patients will use this to determine how close they are to escape

    private void Update()
    {
        if (escaped)
        {
            EscapedUpdate();
        } else
        {
            ContainedUpdate();
        }
    }

    //method to be overwritten by subclass, what the patient does on update when they are escaped
    private void EscapedUpdate()
    {

    }


    //method to be overwritten by subclass, what the patient does on update when they are contained
    private void ContainedUpdate()
    {

    }


    public void cleanUp() { //send the patient back to their room, normally activated by the janitor

    }

}
