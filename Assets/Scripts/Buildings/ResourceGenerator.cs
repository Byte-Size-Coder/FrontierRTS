using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceGenerator : NetworkBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private int resourcePerInterval = 10;
    [SerializeField] private float interval = 2f;

    private float timer;
    private FrontierRTSPlayer player;

    #region Server
    public override void OnStartServer()
    {
        timer = interval;
        player = connectionToClient.identity.GetComponent<FrontierRTSPlayer>();

        health.ServerOnDie += ServerHandleDie;
        GameHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
        GameHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            player.AddResources(resourcePerInterval);
            timer += interval;
        }
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void ServerHandleGameOver()
    {
        enabled = false;
    }

    #endregion
}
