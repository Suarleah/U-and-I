using UnityEngine;


//this is a class for the patient's actual ingame behavior, so movement, attacks, health, etc.
public class Patient : MonoBehaviour
{
    int room; //the room # they are staying at
    bool escaped; //whether or not they're currently escaped
    int patience; //most patients will use this to determine how close they are to escape



    public void cleanUp() { //send the patient back to their room, normally activated by the janitor

    }

}
