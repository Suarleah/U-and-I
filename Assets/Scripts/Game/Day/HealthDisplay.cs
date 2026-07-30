using TMPro;
using UnityEngine;
using Unity.UI;
using UnityEngine.UI;
using System;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private Image blood; private Color bloodRed;
    PlayerStats player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        player = FishNet.InstanceFinder.ClientManager.Connection.FirstObject.gameObject.GetComponent<PlayerStats>();

        bloodRed = new Color(blood.color.r, blood.color.g, blood.color.b, 0);
        blood.color = bloodRed;

        player.health.OnChange += OnChangeHealth;
        
    }

    void OnChangeHealth(int prev, int next, bool asServer)
    {
        bloodRed.a = 1f - (next / 100f);
        blood.color = bloodRed;
    }

    void OnDestroy()
    {
        player.health.OnChange -= OnChangeHealth;
    }
    
}
