using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class FrontierRTSPlayer : NetworkBehaviour
{
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private Building[] buildings;
    [SerializeField] private float buildingRangeLimit = 5f;
    [SerializeField] private Transform cameraTransform;

    private List<Unit> myUnits = new List<Unit>();
    private List<Building> myBuidings = new List<Building>();

    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
    private int resources = 100;

    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStatUpdated))]
    private bool isPartyOwner = false;

    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string displayName;

    private Color teamColor = new Color();

    public event Action<int> ClientOnResourcesUpdated;

    public static event Action ClientOnInfoUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

    public string GetDisplayName()
    {
        return displayName;
    }

    public bool GetIsPartyOwner()
    {
        return isPartyOwner;
    }

    public Transform GetCameraTransform()
    { 
        return cameraTransform; 
    }

    public Color GetTeamColor()
    {
        return teamColor;
    }

    public List<Unit> GetMyUnits ()
    {
        return myUnits;
    }

    public List<Building> GetMyBuildings()
    {
        return myBuidings;
    }

    public int GetResources()
    {
        return resources;
    }



    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
    {
        if (Physics.CheckBox(point + buildingCollider.center, buildingCollider.size / 2, Quaternion.identity, buildingBlockLayer))
        {
            return false;
        }

        foreach (Building building in myBuidings)
        {
            if (BSCUtil.InRange(building.transform.position, point, buildingRangeLimit))
            {
                return true;
            }
        }

        return false;
    }

    #region  Server
    [Server]
    public void SetPartyOwner(bool state)
    {
        isPartyOwner = state;
    }

    [Server]
    public void AddResources(int addedResources)
    {
        resources += addedResources;
    }

    [Server]
    public void RemoveResources(int removedResources)
    {
        resources -= removedResources;
    }

    [Server]
    public void SetDisplayName(string name)
    {
        displayName = name;
    }

    [Server]
    public void SetTeamColor(Color color)
    {
        teamColor = color;
    }
    public override void OnStartServer()
    {
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDeSpawned += ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDeSpawned += ServerHandleBuildingDeSpawned;

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDeSpawned -= ServerHandleUnitDespawned;
        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDeSpawned -= ServerHandleBuildingDeSpawned;
    }

    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 point)
    {
        Building buildingToPlace = null;


        foreach(Building building in buildings)
        {
            if (building.GetBuildingId() == buildingId)
            {
                buildingToPlace = building;
            }
        }

        if (buildingToPlace == null) { return; }

        if (resources < buildingToPlace.GetPrice()) { return; }

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

        if (!CanPlaceBuilding(buildingCollider, point)) { return; }

        GameObject buildingInstance = Instantiate(buildingToPlace.gameObject, point, Quaternion.identity);

        NetworkServer.Spawn(buildingInstance, connectionToClient);

        RemoveResources(buildingToPlace.GetPrice());
    }

    [Command]
    public void CmdStartGame()
    {
        if(!isPartyOwner) { return; }

         ((FrontierRTSNetworkManager)NetworkManager.singleton).StartGame();
    }

    private void ServerHandleUnitSpawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Add(unit);
    }

    private void ServerHandleUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myUnits.Remove(unit);
    }

    private void ServerHandleBuildingSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuidings.Add(building);
    }

    private void ServerHandleBuildingDeSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }

        myBuidings.Remove(building);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        if (NetworkServer.active) { return; }

        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDeSpawned += AuthorityHandleUnitDeSpawned;
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDeSpawned += AuthorityHandleBuildingDeSpawned;
    }

    public override void OnStartClient()
    {
        if (NetworkServer.active) { return; }

        ((FrontierRTSNetworkManager)NetworkManager.singleton).Players.Add(this);

        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopClient()
    {
        ClientOnInfoUpdated?.Invoke();

        if (!isClientOnly) { return; }

        ((FrontierRTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

        if(!isOwned) { return; }

        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDeSpawned -= AuthorityHandleUnitDeSpawned;
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDeSpawned -= AuthorityHandleBuildingDeSpawned;
    }

    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        if (!isOwned) { return; }

        myUnits.Add(unit);
    }

    private void AuthorityHandleUnitDeSpawned(Unit unit)
    {
        if (!isOwned) { return; }

        myUnits.Remove(unit);
    }

    private void AuthorityHandlePartyOwnerStatUpdated(bool oldState, bool newState)
    {
        if(!isOwned) { return; }

        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    private void AuthorityHandleBuildingSpawned(Building building)
    {
        if (!isOwned) { return; }

        myBuidings.Add(building);
    }

    private void AuthorityHandleBuildingDeSpawned(Building building)
    {
        if (!isOwned) { return; }

        myBuidings.Remove(building);
    }

    private void ClientHandleResourcesUpdated(int oldResouce, int newResouce)
    {
        ClientOnResourcesUpdated?.Invoke(newResouce);
    }

    private void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
    {
        ClientOnInfoUpdated?.Invoke();
    }

    #endregion
}
