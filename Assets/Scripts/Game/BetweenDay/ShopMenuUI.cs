using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class ShopMenuUI : MonoBehaviour
{
    public static ShopMenuUI Instance;

    [Header("Layout")]
    public Transform listParent;
    public ShopRowUI rowPrefab;

    [Header("Feedback")]
    public TextMeshProUGUI messageText;
    public float messageDuration = 2f;
    private float messageTimer;

    private readonly List<ShopRowUI> spawnedRows = new List<ShopRowUI>();

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        GameManager.Instance.credits.OnChange += OnCreditsChanged;
        BuildList();
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.credits.OnChange -= OnCreditsChanged;
    }

    void Update()
    {
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f) messageText.text = "";
        }
    }

    private void BuildList()
    {
        foreach (Transform child in listParent) Destroy(child.gameObject);
        spawnedRows.Clear();

        List<ShopEntry> catalog = ShopManager.Instance.catalog;
        for (int i = 0; i < catalog.Count; i++)
        {
            int index = i; // capture for the button closure
            ShopEntry entry = catalog[i];

            ShopRowUI row = Instantiate(rowPrefab, listParent);
            row.nameLabel.text = entry.item.name;
            row.costLabel.text = entry.cost + "cr";
            if (entry.item.inventoryVisual)
                row.icon.sprite = entry.item.inventoryVisual;

            row.buyButton.onClick.AddListener(() => Buy(index));
            spawnedRows.Add(row);
        }

        RefreshAffordability();
    }

    private void Buy(int catalogIndex)
    {
        ShopManager.Instance.TryPurchaseItem(catalogIndex);
    }

    private void OnCreditsChanged(float prev, float next, bool asServer) => RefreshAffordability();

    private void RefreshAffordability()
    {
        List<ShopEntry> catalog = ShopManager.Instance.catalog;
        for (int i = 0; i < spawnedRows.Count && i < catalog.Count; i++)
        {
            spawnedRows[i].buyButton.interactable = GameManager.Instance.credits.Value >= catalog[i].cost;
        }
    }

    public void ShowMessage(string text)
    {
        messageText.text = text;
        messageTimer = messageDuration;
    }
}
