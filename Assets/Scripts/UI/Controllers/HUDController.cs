using BSC.UI;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDController : UIController
{
    [SerializeField] private TMP_Text resourceText;

    private FrontierRTSPlayer player;

    public override void OnUIAdded(object data = null)
    {
        player = NetworkClient.connection.identity.GetComponent<FrontierRTSPlayer>();

        ClientHandleResourcesUpdated(player.GetResources());

        player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
    }

    public override void OnUIRemoved()
    {
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    private void ClientHandleResourcesUpdated(int resources)
    {
        resourceText.text = $"Resources: {resources}";
    }
}
