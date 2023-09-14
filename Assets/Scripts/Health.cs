using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth;

    [Header("Display")]
    [SerializeField] private Unit unit;
    [SerializeField] private GameObject healthBarParent;
    [SerializeField] private Image healthBarImage;

    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currentHealth;

    public event Action ServerOnDie;
    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
        UnitBase.ServerOnPlayerDie += ServerHandleOnPlayerDie;


        if (unit != null && isOwned)
        {
            unit.OnSelected += TurnOnDisplay;
            unit.OnDeselected += TurnOffDisplay;
        }
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandleOnPlayerDie;

        if (unit != null && isOwned)
        {
            unit.OnSelected -= TurnOnDisplay;
            unit.OnDeselected -= TurnOffDisplay;
        }
    }


    [Server]
    private void ServerHandleOnPlayerDie(int connectionId)
    {
        if (connectionToClient.connectionId != connectionId) { return; }

        DealDamage(currentHealth);
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if (currentHealth == 0) { return; }

        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if (currentHealth != 0) { return; }

        ServerOnDie?.Invoke();

        Debug.Log("We Died");
    }

    #endregion

    #region Client


    private void OnMouseEnter()
    {
        if (isOwned) { return; }
        TurnOnDisplay();
    }

    private void OnMouseExit()
    {
        if (isOwned) { return; }
        TurnOffDisplay();
    }

    private void TurnOnDisplay()
    {
        healthBarParent.SetActive(true);
    }

    private void TurnOffDisplay()
    {
        healthBarParent.SetActive(false);
    }

    private void HandleHealthUpdated(int oldHelth, int newHealth)
    {
        healthBarImage.fillAmount = (float)newHealth / maxHealth;
    }

    #endregion
}
