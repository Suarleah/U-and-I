using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One ad. Click the X to close it (only for you).
public class AdPopup : MonoBehaviour
{
    public Image art;
    public TextMeshProUGUI placeholderLabel;              // only used when there's no sprite
    public Button closeButton;
    public Vector2 closeDelayRange = new Vector2(0f, 0f); // set e.g. (0, 1.5) so some ads make you wait for the X
    public float popInTime = 0.12f;

    public void Setup(Sprite sprite, Vector2 size)
    {
        art.sprite = sprite;
        art.color = Color.white;
        if (placeholderLabel) placeholderLabel.gameObject.SetActive(false);
        ((RectTransform)transform).sizeDelta = size;
    }

    public void SetupPlaceholder(string text, Color color, Vector2 size)
    {
        art.sprite = null;
        art.color = color;
        if (placeholderLabel)
        {
            placeholderLabel.gameObject.SetActive(true);
            placeholderLabel.text = text;
        }
        ((RectTransform)transform).sizeDelta = size;
    }

    void Start()
    {
        closeButton.onClick.AddListener(Close);
        StartCoroutine(PopIn());

        float delay = Random.Range(closeDelayRange.x, closeDelayRange.y);
        if (delay > 0f) StartCoroutine(ShowCloseAfter(delay));
    }

    private IEnumerator PopIn()
    {
        for (float t = 0f; t < popInTime; t += Time.deltaTime)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, t / popInTime);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    private IEnumerator ShowCloseAfter(float delay)
    {
        closeButton.gameObject.SetActive(false);
        yield return new WaitForSeconds(delay);
        closeButton.gameObject.SetActive(true);
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
