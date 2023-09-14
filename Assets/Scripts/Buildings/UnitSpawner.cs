using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health;
    [SerializeField] private Unit unitPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private TMP_Text remainingUnitTexts;
    [SerializeField] private Image unitProgressImage;
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7;
    [SerializeField] private float unitSpawnDuration = 3f;

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;

    private void Update()
    {
        if (isServer)
        {
            ProduceUnit();
        }

        if (isClient)
        {
            UpdateTimerDisplay();
        }
    }

    #region Server

    public override void OnStartServer()
    {
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }


    [Command]
    private void CmdSpawnUnit()
    {
       if (queuedUnits == maxUnitQueue) { return; }

        FrontierRTSPlayer player = connectionToClient.identity.GetComponent<FrontierRTSPlayer>();

        if (player.GetResources() < unitPrefab.GetResourceCost()) { return; }

        player.RemoveResources(unitPrefab.GetResourceCost());

        queuedUnits++;
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    private void ProduceUnit()
    {
        if (queuedUnits == 0) { return; }

        unitTimer += Time.deltaTime;

        if (unitTimer < unitSpawnDuration) { return; }

        GameObject unitInstance = Instantiate(unitPrefab.gameObject, spawnPoint.position, spawnPoint.rotation);

        NetworkServer.Spawn(unitInstance, connectionToClient);

        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = spawnPoint.position.y;
        
        Unit unit = unitInstance.GetComponent<Unit>();

        unit.ServerMove(spawnPoint.position + spawnOffset);

        queuedUnits--;

        unitTimer = 0;
    }
    #endregion

    #region Client

    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;

        if (newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(
                unitProgressImage.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.1f
            );
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (!isOwned) 
        {
            return;
        }

        CmdSpawnUnit();
    }

    private void ClientHandleQueuedUnitsUpdated(int oldvalue, int newvalue)
    {
        queuedUnits = newvalue;
        remainingUnitTexts.text = newvalue.ToString();
    }

    #endregion
}
