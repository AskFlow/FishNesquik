using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : NetworkBehaviour
{
    public Image backgroundImage;
    public TextMeshProUGUI youText;
    public Toggle toggle;
    public Color isPlayerColor;
    public bool isPlayer;
    public bool isReady;


    public override void OnStartClient()
    {
        base.OnStartClient();
        if(IsOwner)
        {
            backgroundImage.color = isPlayerColor;
            youText.gameObject.SetActive(true);
        }
    }

    public void ToggleToggle()
    {
        toggle.isOn = !toggle.isOn;
    }

    void Update()
    {
        //if (isPlayer)
        //{
        //    backgroundImage.color = isPlayerColor;
        //    youText.enabled = true;
        //}
        //else
        //{
        //    youText.enabled = false;

        //}
        //toggle.isOn = isReady;
    }
}