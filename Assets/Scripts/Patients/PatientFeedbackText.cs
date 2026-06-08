using UnityEngine;
using TMPro;
using FishNet;
using FishNet.Object;

public class PatientFeedbackText : NetworkBehaviour
{
    //when you interact with a Patient, they might give feedback on your interaction, this is the visual element of that
    [ObserversRpc]
    public void Begin()
    {
        gameObject.SetActive(true);
        gameObject.GetComponentInChildren<TextMeshProUGUI>().alpha = 1; //just temp, its to set opacity to 0
        
    }

    [ObserversRpc]
    // Update is called once per frame
    void Update()
    {
        transform.position += Time.deltaTime * (new Vector3(0, 1, 0));
        gameObject.GetComponentInChildren<TextMeshProUGUI>().alpha -= Time.deltaTime;
        if (gameObject.GetComponentInChildren<TextMeshProUGUI>().alpha <= 0)
        {
            Despawn(base.NetworkObject);
        }
        
    }
}
