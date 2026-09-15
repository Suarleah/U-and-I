using TMPro;
using UnityEngine;
using UnityEngine.UI;

// --- NEW FILE (shop) ---
// Thin holder for one row in the shop menu's item list, so ShopMenuUI can set fields
// directly instead of fishing for children by name/type.
public class ShopRowUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI costLabel;
    public Button buyButton;
}
