using UnityEngine;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Text joinCodeDisplay;
    public TMP_InputField joinCodeInput;
    public GameObject menu;
    public GameObject lobby;

    async void Start()
    {
        await RelayManager.Instance.InitializeAsync();
    }

    public async void OnHostClicked()
    {
        string code = await RelayManager.Instance.CreateRelayAsync(RelayManager.Instance.maxPlayers);
        // get code from ro4;ieiirmnpv;ivb
        joinCodeDisplay.text = "Secret code: " + code;
        menu.SetActive(false);
        lobby.SetActive(true);
    }

    public async void OnJoinClicked()
    {
        await RelayManager.Instance.JoinRelayAsync(joinCodeInput.text.Trim().ToUpper());
        menu.SetActive(false);
        lobby.SetActive(true);
        // GET CODE FROM THE RELAY MANAGER
    }
}