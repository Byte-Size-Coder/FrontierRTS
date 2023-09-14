using BSC.UI;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyHubController : UIController
{
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button startButton;

    [SerializeField] private TMP_Text[] playerNameText;
    public override void OnUIAdded(object data = null)
    {
        leaveButton.onClick.AddListener(OnLeaveButtonClicked);
        startButton.onClick.AddListener(OnStartButtonClicked);

        FrontierRTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
        FrontierRTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
    }

    public override void OnUIRemoved()
    {
        leaveButton.onClick.RemoveListener(OnLeaveButtonClicked);
        startButton.onClick.RemoveListener(OnStartButtonClicked);

        FrontierRTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
        FrontierRTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    private void OnLeaveButtonClicked()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }

        UIStack.Instance.Pop();
    }

    private void AuthorityHandlePartyOwnerStateUpdated(bool state)
    {
        startButton.gameObject.SetActive(state);
    }

    private void OnStartButtonClicked()
    {
        NetworkClient.connection.identity.GetComponent<FrontierRTSPlayer>().CmdStartGame();
    }

    private void ClientHandleInfoUpdated ()
    {
        List<FrontierRTSPlayer> players = ((FrontierRTSNetworkManager)NetworkManager.singleton).Players;

        for (int i = 0; i < players.Count; i++)
        {
            playerNameText[i].text = players[i].GetDisplayName();
        }

        for (int i = players.Count; i < playerNameText.Length; i++)
        {
            playerNameText[i].text = "Waiting for Player...";
        }

        startButton.interactable = players.Count > 1;
    }
}
