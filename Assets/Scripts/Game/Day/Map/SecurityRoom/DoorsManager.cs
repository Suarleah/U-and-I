using System.Collections.Generic;
using UnityEngine;

public class DoorsManager : MonoBehaviour
{
    //this is the script that manages the doors (ie which doors are on cooldown at any given time, which doors are opened/closed for the janitor)
   
   public List<bool> doors = new List<bool>(); //keeps track of what doors are down

}
