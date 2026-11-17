using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Local pop-up ad layer. Sorts under every other UI canvas, so menus stay usable but the game gets covered.
public class AdOverlay : MonoBehaviour
{
    public static AdOverlay Instance;

    public RectTransform adArea;                         // usually this canvas's own RectTransform
    public AdPopup adPrefab;
    public Sprite[] adSprites;                           // random art per ad. leave empty for placeholder color + text ads
    public string[] placeholderTexts = { "HOT SINGLES IN YOUR WARD", "DOCTORS HATE THIS ONE TRICK", "FREE RAM DOWNLOAD" };
    public Vector2 widthRange = new Vector2(250f, 450f); // canvas units. height follows the sprite's aspect
    public int maxAds = 30;                              // past this the oldest ad is removed, so it can't tank performance

    public int placementTries = 15;                      // re-rolls the position until the X isn't under other UI

    private readonly List<AdPopup> ads = new List<AdPopup>();
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
    private Canvas canvas;

    void Awake()
    {
        Instance = this;
        if (adArea == null) adArea = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
    }

    public void SpawnAds(int count)
    {
        for (int i = 0; i < count; i++) SpawnAd();
    }

    public void SpawnAd()
    {
        ads.RemoveAll(a => a == null); // ones the player closed
        if (ads.Count >= maxAds)
        {
            Destroy(ads[0].gameObject);
            ads.RemoveAt(0);
        }

        Sprite sprite = adSprites.Length > 0 ? adSprites[Random.Range(0, adSprites.Length)] : null;
        float width = Random.Range(widthRange.x, widthRange.y);
        float aspect = sprite ? sprite.rect.height / sprite.rect.width : 0.75f;
        Vector2 size = new Vector2(width, width * aspect);

        AdPopup ad = Instantiate(adPrefab, adArea); // newest ends up on top
        if (sprite) ad.Setup(sprite, size);
        else ad.SetupPlaceholder(placeholderTexts[Random.Range(0, placeholderTexts.Length)], Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.8f, 1f), size);

        // random spot fully inside adArea (rect coords are relative to the pivot, same as localPosition).
        // Re-roll if the X would land under other UI, since then it could never be clicked.
        Rect r = adArea.rect;
        for (int attempt = 0; attempt < placementTries; attempt++)
        {
            float x = Random.Range(r.xMin + size.x * 0.5f, Mathf.Max(r.xMin + size.x * 0.5f, r.xMax - size.x * 0.5f));
            float y = Random.Range(r.yMin + size.y * 0.5f, Mathf.Max(r.yMin + size.y * 0.5f, r.yMax - size.y * 0.5f));
            ad.transform.localPosition = new Vector3(x, y, 0f);
            if (CloseButtonReachable(ad)) break;
        }

        ads.Add(ad);
    }

    // true if a click on the ad's X would reach this ad (or another ad) instead of some other UI on top of it
    private bool CloseButtonReachable(AdPopup ad)
    {
        if (EventSystem.current == null || ad.closeButton == null) return true;

        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, ad.closeButton.transform.position);

        var pointer = new PointerEventData(EventSystem.current) { position = screen };
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointer, raycastResults); // sorted, topmost first
        if (raycastResults.Count == 0) return true;
        return raycastResults[0].gameObject.GetComponentInParent<AdPopup>() != null;
    }

    public void ClearAll()
    {
        foreach (AdPopup ad in ads)
        {
            if (ad) Destroy(ad.gameObject);
        }
        ads.Clear();
    }
}
