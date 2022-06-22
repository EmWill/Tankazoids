using FishNet;
using FishNet.Component.ColliderRollback;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class Tank : NetworkBehaviour
{
    public GameObject defaultWeapon0Prefab;
    public GameObject defaultWeapon1Prefab;
    public GameObject defaultBodyPrefab;
    public GameObject defaultTreadPrefab;

    public GameObject weaponContainer;
    public GameObject bodyContainer;
    public GameObject treadContainer;

    private GameObject _weapon0Object;
    private GameObject _weapon1Object;
    private GameObject _bodyObject;
    private GameObject _treadObject;

    private AbstractWeapon _weapon0Component;
    private AbstractWeapon _weapon1Component;
    private AbstractBody _bodyComponent;
    private AbstractTread _treadsComponent;

    public StatManager maxHealthModifiers { get; private set; }
    public StatManager moveSpeedModifiers { get; private set; }
    public StatManager cooldownModifiers { get; private set; }
    public StatManager maxHeatModifiers { get; private set; }
    public StatManager damageModifiers { get; private set; }
    public StatManager speedModifiers { get; private set; }

    private float _heat;

    [SyncVar]
    private float _health;

    // synced by replicate
    private bool _sprinting = false;

    // this is synced manually
    private bool _overheated = false;

    private bool _dead = false;

    private MapManager _mapManager;

    public delegate void OnHitHandler(ref float dmg);
    public event OnHitHandler OnHit;

    public Rigidbody2D rigidbody2d { get; private set; }

    #region Lifecycle
    private void Awake()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();

        maxHealthModifiers = new();
        moveSpeedModifiers = new();
        cooldownModifiers = new();
        maxHeatModifiers = new();
        damageModifiers = new();
        speedModifiers = new();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        InstanceFinder.TimeManager.OnTick += OnTick;
        InstanceFinder.TimeManager.OnPostTick += OnPostTick;
    }

    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
        {
            InstanceFinder.TimeManager.OnTick -= OnTick;
            InstanceFinder.TimeManager.OnPostTick -= OnPostTick;
        }
    }

    public void setMapManager(MapManager manager)
    {
        _mapManager = manager;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        EquipWeapon0(defaultWeapon0Prefab);
        EquipWeapon1(defaultWeapon1Prefab);
        EquipBody(defaultBodyPrefab);
        EquipTread(defaultTreadPrefab);

        _health = GetMaxHealth();

        rigidbody2d.bodyType = RigidbodyType2D.Dynamic;
    }
    #endregion Lifecycle

    // todo could be a good idea to formalize some of this stuff and make it easier to understand
    private void OnTick()
    {
        if (base.IsDeinitializing) return;

        // not replicated!!
        if (base.IsOwner)
        {

            InputData inputData = GetInputData();
            _weapon0Component.OnTankTick(inputData);
            _weapon1Component.OnTankTick(inputData);
            _bodyComponent.OnTankTick(inputData);
            _treadsComponent.OnTankTick(inputData);

            if (inputData.bodyButton.IsButtonDown() && TimeManager.LocalTick % 20 == 0)
            {
                RemoveHealth(999);
            }

            if (inputData.swapButton.IsButtonDown())
            {
                SwapWeapons();
            }
            if (inputData.weapon0Button.IsPressed())
            {
                _weapon0Component.ActivateAbility(base.RollbackManager.PreciseTick, transform.position, inputData);
            }
            if (inputData.weapon1Button.IsPressed())
            {
                _weapon1Component.ActivateAbility(base.RollbackManager.PreciseTick, transform.position, inputData);
            }

            // handle rollback locally
            Reconcile(default, false);
            Replicate(inputData, false);
        }

        if (base.IsServer)
        {
            // handle rollback on server
            Replicate(default, true);
        }
    }

    private void OnPostTick()
    {
        if (base.IsDeinitializing) return;

        Reconcile(GetReconcileData(), true);
    }

    [Replicate]
    private void Replicate(Tank.InputData inputData, bool asServer, bool replaying = false)
    {
        if (inputData.sprintButton.IsPressed() != _sprinting)
        {
            if (inputData.sprintButton.IsPressed())
            {
                speedModifiers.AddMultiplier(1.5f);
                _sprinting = true;
            }
            else
            {
                speedModifiers.RemoveMultiplier(1.5f);
                _sprinting = false;
            }
        }

        ProcessHeatOnTick();

        _treadsComponent.DecayVelocity();
        _treadsComponent.DecayAngularVelocity();

        _treadsComponent.HandleMovement(inputData);

        Vector3 difference = inputData.worldTargetPos - new Vector3(weaponContainer.transform.position.x, weaponContainer.transform.position.y, 0);
        weaponContainer.transform.eulerAngles = new Vector3(Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg - 90, -90, -90);
    }

    [Reconcile]
    private void Reconcile(ReconcileData reconcileData, bool asServer)
    {
        transform.position = reconcileData.position;
        transform.rotation = reconcileData.rotation;

        rigidbody2d.velocity = reconcileData.rigidbodyVelocity;
        rigidbody2d.angularVelocity = reconcileData.rigidbodyAngularVelocity;

        Debug.Log(_heat - reconcileData.heat);
        SetHeat(reconcileData.heat);
        speedModifiers = new StatManager(reconcileData.speedModifierBonus, speedModifiers.Multiplier);

        weaponContainer.transform.rotation = reconcileData.rotation;

        _weapon0Component.HandleReconcileData(new Reader(reconcileData.weapon0ReconcileData, base.NetworkManager));
        _weapon1Component.HandleReconcileData(new Reader(reconcileData.weapon1ReconcileData, base.NetworkManager));
        _bodyComponent.HandleReconcileData(new Reader(reconcileData.bodyReconcileData, base.NetworkManager));
        _treadsComponent.HandleReconcileData(new Reader(reconcileData.treadsReconcileData, base.NetworkManager));
    }

    public void RaiseOnHitEvent()
    {
    }
}
