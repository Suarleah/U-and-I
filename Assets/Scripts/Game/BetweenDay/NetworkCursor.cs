using FishNet.Object;
using FishNet.Object.Synchronizing;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NetworkCursor : NetworkBehaviour
{

    private Image myColon;
    private RectTransform myRect;
    private Camera canvasRefCam; private Canvas myCanvas; private RectTransform canvasRect;
    readonly SyncVar<Color> netColor = new SyncVar<Color>(Color.white);
    async void Awake() 
    {
        
        // fuck awake actually
    }
    public override void OnStartClient() // After server and still yes object
    {
        base.OnStartClient();

        if (IsOwner)
        {
            VoteManager voteManager = FindFirstObjectByType<VoteManager>();
            ChangeColorServer(voteManager.playerColors[voteManager.colorIndex.Value]);

            myCanvas = voteManager.voteCanvas; // srupid a fucking a netowrking a
            canvasRect = myCanvas.GetComponent<RectTransform>();
            canvasRefCam = myCanvas.worldCamera;

            myColon = gameObject.GetComponent<UnityEngine.UI.Image>();
            myRect = GetComponent<RectTransform>();
            netColor.OnChange += OnColorChanged;
        }
    }
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        Vector3 screenPos = Mouse.current.position.ReadValue();
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvasRefCam, out Vector2 localPoint);
        // I can't beleieve this is an actual method that exists
        myRect.localPosition = new Vector3(localPoint.x, localPoint.y, myRect.localPosition.z);
        // Relative to my parent
    }

    [ServerRpc] // You habe to change syncVar in server
    private void ChangeColorServer(Color c)
    {
        netColor.Value = c; // This updates automatically on clients
    }
    private void OnColorChanged(Color prev, Color next, bool asServer)
    { // Whenever a color changes, if I'm not the owner of the object who changed then half its opacity
        if (!IsOwner)
            myColon.color = new Color(next.r, next.g, next.b, 0.5f); // half opacity for plebians
        else // If I do own the object
            myColon.color = next; // full opacity for yourself
    }
}
