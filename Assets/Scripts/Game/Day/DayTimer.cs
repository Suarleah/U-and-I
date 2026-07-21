using TMPro;
using UnityEngine;

public class DayTimer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        GameManager.Instance.daytime.OnChange += OnChangeTimer;
    }


    void OnChangeTimer(float prev, float next, bool asServer)
    {
        
        text.text = "time: " + (int)next;
        if (next <= 0)
        {
            GameManager.Instance.AllPlayersClocked();
        }
    }

    void OnDestroy()
    {
        GameManager.Instance.daytime.OnChange -= OnChangeTimer;
    }
}
