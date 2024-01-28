using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : NetworkBehaviour
{
    public Image backgroundImage;
    public TextMeshProUGUI youText;
    public Toggle toggle;
    public GameObject readyButton;
    public Color isPlayerColor;
    public bool isPlayer;
    
    [SyncVar] public bool isReady;


    public override void OnStartClient()
    {
        base.OnStartClient();
        if(IsOwner)
        {
            backgroundImage.color = isPlayerColor;
            youText.gameObject.SetActive(true);
            readyButton.gameObject.SetActive(true);
        }
        else
        {
            readyButton.gameObject.SetActive(false);

        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void ToggleServerRpc()
    {
        isReady = !isReady;
    }


    public void ToggleToggle()
    {
        ToggleServerRpc();
    }

    void Update()
    {
        toggle.isOn = isReady;

    }
}
