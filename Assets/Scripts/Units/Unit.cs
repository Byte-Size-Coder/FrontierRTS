using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [SerializeField] private UnitMovement unitMovement;
    [SerializeField] private Targeter targeter;
    [SerializeField] private UnityEvent onSelected;
    [SerializeField] private UnityEvent onDeselected;

    // Notify server when unit is spawned and despawned;
    public static event Action<Unit> ServerOnUnitSpawned;
    public static event Action<Unit> ServerOnUnitDeSpawned;

    public static event Action<Unit> AuthorityOnUnitSpawned;
    public static event Action<Unit> AuthorityOnUnitDeSpawned;

    public UnitMovement GetUnitMovement()
    {
        return unitMovement;
    }

    public Targeter GetTargeter()
    {
        return targeter;
    }

    #region Server
    public override void OnStartServer()
    {
        ServerOnUnitSpawned.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnUnitDeSpawned.Invoke(this);
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        if (!isClientOnly || !isOwned) { return; }

        Debug.Log(AuthorityOnUnitSpawned);

        AuthorityOnUnitSpawned.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !isOwned) { return; }

        AuthorityOnUnitDeSpawned.Invoke(this);
    }

    [Client]
    public void Select()
    {
        if (!isOwned) { return; }

        onSelected.Invoke();
    }

    [Client]
    public void DeSelect()
    {
        if (!isOwned) { return; }

        onDeselected.Invoke();
    }
    #endregion
}
