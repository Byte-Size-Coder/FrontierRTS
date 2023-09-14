using BSC.UI;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyController : UIController
{
    [SerializeField] private Button joinButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_InputField addressInput;
    public override void OnUIAdded(object data = null)
    {
        joinButton.onClick.AddListener(OnJoinButtonClicked);
        closeButton.onClick.AddListener(OnCloseButtonClicked);

        FrontierRTSNetworkManager.ClientOnConnected += HandleClientConnected;
        FrontierRTSNetworkManager.ClientOnDisconnected += HandleClientDisconnected;
    }

    public override void OnUIRemoved()
    {
        joinButton.onClick.RemoveListener(OnJoinButtonClicked);
        closeButton.onClick.RemoveListener(OnCloseButtonClicked);

        FrontierRTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        FrontierRTSNetworkManager.ClientOnDisconnected -= HandleClientDisconnected;
    }

    private void OnJoinButtonClicked()
    {
        if(addressInput.text == "") { return; }

        string address = addressInput.text;

        NetworkManager.singleton.networkAddress = address;
        NetworkManager.singleton.StartClient();

        joinButton.interactable = false;
    }

    private void HandleClientConnected()
    {
        joinButton.interactable = true;

        UIStack.Instance.Push("LobbyHub");
    }

    private void HandleClientDisconnected()
    {
        joinButton.interactable = true;
    }

    private void OnCloseButtonClicked()
    {
        UIStack.Instance.Pop();
    }
}
