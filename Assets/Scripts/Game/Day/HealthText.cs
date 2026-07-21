using TMPro;
using UnityEngine;

public class HealthText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    PlayerStats player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = FishNet.InstanceFinder.ClientManager.Connection.FirstObject.gameObject.GetComponent<PlayerStats>();
        text.text = "health: " + player.health.Value;
        player.health.OnChange += OnChangeHealth;
    }

    void OnChangeHealth(int prev, int next, bool asServer)
    {
        text.text = "health: " + (int)next;
    }

    void OnDestroy()
    {
        player.health.OnChange -= OnChangeHealth;
    }
    
}
