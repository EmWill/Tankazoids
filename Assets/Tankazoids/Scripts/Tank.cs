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


public class Tank : NetworkBehaviour
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

    [SyncVar]
    private float _heat;

    [SyncVar]
    private float _health;

    private bool _sprinting = false;

    // not good for these to be public.. should be private set but idk if that is possible with syncvars
    [SyncVar]
    private bool _overheated = false;

    private bool _dead = false;

    private MapManager _mapManager;

    public delegate void OnHitHandler(ref float dmg);
    public event OnHitHandler OnHit;

    public Rigidbody2D rigidbody2d { get; private set; }

    public struct InputData
    {
        public Vector3 worldTargetPos;

        public Vector2 directionalInput;

        public bool weapon0Pressed;
        public bool weapon1Pressed;
        public bool bodyPressed;
        public bool treadPressed;
        public bool sprintPressed;
        public bool swapPressed;

        public InputData(Vector3 worldTargetPos, Vector2 directionalInput, bool weapon0Pressed, bool weapon1Pressed, 
            bool bodyPressed, bool treadPressed, bool sprintPressed, bool swapPressed)
        {
            this.worldTargetPos = worldTargetPos;

            this.directionalInput = directionalInput;

            this.weapon0Pressed = weapon0Pressed;
            this.weapon1Pressed = weapon1Pressed;
            this.bodyPressed = bodyPressed;
            this.treadPressed = treadPressed;
            this.sprintPressed = sprintPressed;
            this.swapPressed = swapPressed;
        }
    }

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

    #region Control
    private InputData GetInputData()
    {
        // todo get a reference to the camera... maybe
        Camera camera = Camera.main;

        // todo this is bad?
        if (camera == null)
        {
            return default;
        }

        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPointCoords = camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -camera.transform.position.z));
        Vector3 mouseWorldCoords = new(worldPointCoords.x, worldPointCoords.y, 0);

        return new InputData(
                mouseWorldCoords,
                new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
                Input.GetButton("Weapon0"),
                Input.GetButton("Weapon1"),
                Input.GetButton("Body"),
                Input.GetButton("Tread"),
                Input.GetButton("Sprint"),
                Input.GetButton("Swap")
            );
    }

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

            if (inputData.bodyPressed && TimeManager.LocalTick % 20 == 0)
            {
                RemoveHealth(999);
            }

            if (inputData.swapPressed)
            {
                SwapWeapons();
            }
            if (inputData.weapon0Pressed)
            {
                _weapon0Component.ActivateAbility(base.RollbackManager.PreciseTick, transform.position, inputData);
            }
            if (inputData.weapon1Pressed)
            {
                _weapon1Component.ActivateAbility(base.RollbackManager.PreciseTick, transform.position, inputData);
            }

            // handle rollback locally
            Reconcile(default, false);
            Replicate(inputData, false);
        }

        if (base.IsServer)
        {
            ProcessHeatOnTick();

            // handle rollback on server
            Replicate(default, true);
        }
    }

    private void OnPostTick()
    {
        if (base.IsDeinitializing) return;

        // we could save on this allocation if we wanted to make the interface less nice... maybe we do
        Writer weapon0Writer = new();
        Writer weapon1Writer = new();
        Writer bodyWriter = new();
        Writer treadsWriter = new();

        // ask the components to write their data to the writers
        _weapon0Component.GetReconcileData(weapon0Writer);
        _weapon1Component.GetReconcileData(weapon1Writer);
        _bodyComponent.GetReconcileData(bodyWriter);
        _treadsComponent.GetReconcileData(treadsWriter);

        ReconcileData reconcileData = new ReconcileData(
            transform.position,
            transform.rotation,
            weaponContainer.transform.rotation,
            rigidbody2d.velocity,
            rigidbody2d.angularVelocity,
            speedModifiers,

            weapon0Writer.GetArraySegment().Array,
            weapon1Writer.GetArraySegment().Array,
            bodyWriter.GetArraySegment().Array,
            treadsWriter.GetArraySegment().Array
            );

        Reconcile(reconcileData, true);
    }

    #endregion Control

    #region Equipping
    [ServerRpc]
    public void SwapWeapons()
    {
        print("we swapping no stopping");
        GameObject leftHand = _weapon0Object;
        EquipWeapon0(_weapon1Object);
        EquipWeapon1(leftHand);
    }
    // todo mkae this server_spawnweapon and put equip elsewhere?
    public void EquipWeapon0(GameObject prefab) {
        GameObject oldWeapon = _weapon0Object;
        NetworkBehaviour oldWeaponComponent = _weapon0Component;


        _weapon0Object = Instantiate(prefab, weaponContainer.transform);
        InstanceFinder.ServerManager.Spawn(_weapon0Object.GetComponent<NetworkObject>(), base.Owner);

        _weapon0Component = _weapon0Object.GetComponent<AbstractWeapon>();
        _weapon0Component.OnEquip(this);
        UpdateClientWeapon0(base.Owner, _weapon0Object, _weapon0Component);


        if (oldWeapon == null)
        {
            return;
        }

        oldWeaponComponent.Despawn();
        Destroy(oldWeapon);
    }

    public void EquipWeapon1(GameObject prefab)
    {
        GameObject oldWeapon = _weapon1Object;
        NetworkBehaviour oldWeaponComponent = _weapon1Component;


        _weapon1Object = Instantiate(prefab, weaponContainer.transform);
        _weapon1Object.transform.localScale = new Vector3(_weapon1Object.transform.localScale.x, _weapon1Object.transform.localScale.y, -_weapon1Object.transform.localScale.z);
        InstanceFinder.ServerManager.Spawn(_weapon1Object.GetComponent<NetworkObject>(), base.Owner);

        _weapon1Component = _weapon1Object.GetComponent<AbstractWeapon>();
        _weapon1Component.OnEquip(this);
        UpdateClientWeapon1(base.Owner, _weapon1Object, _weapon1Component);


        if (oldWeapon == null)
        {
            return;
        }

        oldWeaponComponent.Despawn();
        Destroy(oldWeapon);
    }

    public void EquipBody(GameObject prefab)
    {
        GameObject oldBody = _bodyObject;
        NetworkBehaviour oldBodyComponent = _bodyComponent;

        _bodyObject = Instantiate(prefab, bodyContainer.transform);
        InstanceFinder.ServerManager.Spawn(_bodyObject.GetComponent<NetworkObject>(), base.Owner);

        _bodyComponent = _bodyObject.GetComponent<AbstractBody>();
        _bodyComponent.OnEquip(this);
        UpdateClientBody(base.Owner, _bodyObject, _bodyComponent);

        if (oldBody == null)
        {
            return;
        }

        oldBodyComponent.Despawn();
        Destroy(oldBody);
    }

    public void EquipTread(GameObject prefab)
    {
        GameObject oldTread = _treadObject;
        NetworkBehaviour oldTreadComponent = _treadsComponent;

        _treadObject = Instantiate(prefab, treadContainer.transform);
        InstanceFinder.ServerManager.Spawn(_treadObject.GetComponent<NetworkObject>(), base.Owner);

        _treadsComponent = _treadObject.GetComponent<AbstractTread>();
        _treadsComponent.OnEquip(this);
        UpdateClientTread(_treadObject, _treadsComponent);

        if (oldTread == null)
        {
            return;
        }

        oldTreadComponent.Despawn();
        Destroy(oldTread);
    }

    [ObserversRpc(BufferLast = true)]
    public void UpdateClientWeapon0(NetworkConnection conn, GameObject weaponObject, AbstractWeapon weaponComponent)
    {
        weaponObject.transform.SetParent(weaponContainer.transform);

        _weapon0Object = weaponObject;
        _weapon0Component = weaponComponent;

        _weapon0Component.OnEquip(this);
    }

    [ObserversRpc(BufferLast = true)]
    public void UpdateClientWeapon1(NetworkConnection conn, GameObject weaponObject, AbstractWeapon weaponComponent)
    {
        weaponObject.transform.SetParent(weaponContainer.transform);

        _weapon1Object = weaponObject;
        _weapon1Component = weaponComponent;

        _weapon1Component.OnEquip(this);
    }

    [ObserversRpc(BufferLast = true)]
    public void UpdateClientBody(NetworkConnection conn, GameObject bodyObject, AbstractBody bodyComponent)
    {
        bodyObject.transform.SetParent(bodyContainer.transform);

        _bodyObject = bodyObject;
        _bodyComponent = bodyComponent;

        _bodyComponent.OnEquip(this);
    }

    [ObserversRpc(BufferLast = true)]
    public void UpdateClientTread(GameObject treadObject, AbstractTread treadComponent)
    {
        treadObject.transform.SetParent(treadContainer.transform);

        _treadObject = treadObject;
        _treadsComponent = treadComponent;

        _treadsComponent.OnEquip(this);
    }
    #endregion Equipping

    #region Rollback

    public struct ReconcileData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Quaternion weaponRotation;

        public Vector3 rigidbodyVelocity;
        public float rigidbodyAngularVelocity;

        public StatManager speedModifiers;

        public byte[] weapon0ReconcileData;
        public byte[] weapon1ReconcileData;
        public byte[] bodyReconcileData;
        public byte[] treadsReconcileData;

        public ReconcileData(Vector3 position, Quaternion rotation, Quaternion weaponRotation,
            Vector3 rigidbodyVelocity,
            float rigidbodyAngularVelocity,
            StatManager speedModifiers,
            byte[] weapon0ReconcileData,
            byte[] weapon1ReconcileData, 
            byte[] bodyReconcileData,
            byte[] treadsReconcileData
            )
        {
            this.position = position;
            this.rotation = rotation;

            this.rigidbodyVelocity = rigidbodyVelocity;
            this.rigidbodyAngularVelocity = rigidbodyAngularVelocity;

            this.speedModifiers = speedModifiers;

            this.weaponRotation = weaponRotation;

            this.weapon0ReconcileData = weapon0ReconcileData;
            this.weapon1ReconcileData = weapon1ReconcileData;
            this.bodyReconcileData = bodyReconcileData;
            this.treadsReconcileData = treadsReconcileData;
        }
    }

    [Replicate]
    private void Replicate(Tank.InputData inputData, bool asServer, bool replaying = false)
    {
        if (inputData.sprintPressed != _sprinting)
        {
            if (inputData.sprintPressed)
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

        _treadsComponent.DecayVelocity();
        _treadsComponent.DecayAngularVelocity();

        Vector3 pos = transform.position;
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

        speedModifiers = reconcileData.speedModifiers;

        weaponContainer.transform.rotation = reconcileData.rotation;

        _weapon0Component.HandleReconcileData(new Reader(reconcileData.weapon0ReconcileData, base.NetworkManager));
        _weapon1Component.HandleReconcileData(new Reader(reconcileData.weapon1ReconcileData, base.NetworkManager));
        _bodyComponent.HandleReconcileData(new Reader(reconcileData.bodyReconcileData, base.NetworkManager));
        _treadsComponent.HandleReconcileData(new Reader(reconcileData.treadsReconcileData, base.NetworkManager));
    }
    #endregion Rollback

    #region Health
    public float GetHealth()
    {
        return _health;
    }

    public float GetMaxHealth()
    {
        return maxHealthModifiers.CalculateStat(_bodyComponent.MaxHealth);
    }

    [Server]
    public void AddHealth(float amount)
    {
        if (amount <= 0)
        {
            Debug.LogError("healed 0 or negative!?");
            Debug.LogError("doggy doggy WHAT now!?");
        }

        _health = Math.Min(_health + amount, GetMaxHealth());
    }

    [Server]
    public void RemoveHealth(float amount)
    {
        if (amount <= 0)
        {
            Debug.LogError("took 0 or negative damage!?");
        }

        _health = Math.Max(_health - amount, 0);

        if (_health <= 0)
        {
            Die();
        }
    }

    [Server]
    public void Die()
    {
        if (!_dead && base.IsServer)
        {
            _mapManager.Respawn(this);
            _dead = true;
        }
    }
    #endregion Health

    #region Heat
    public float GetHeat()
    {
        return _heat;
    }

    public float GetMaxHeat()
    {
        return maxHeatModifiers.CalculateStat(_bodyComponent.MaxHeat);
    }

    [Server]
    private void ProcessHeatOnTick()
    {
        if (_sprinting)
        {
            AddHeat(30 * (float)base.TimeManager.TickDelta);
        }
        else
        {
            if (!_overheated)
            {
                RemoveHeat(10 * (float)base.TimeManager.TickDelta);
            } else
            {
                RemoveHeat(30 * (float)base.TimeManager.TickDelta);
            }
        }
    }

    [Server]
    public void AddHeat(float amount)
    {
        if (amount <= 0)
        {
            Debug.LogError("added 0 or negative heat!?");
        }

        _heat += amount;

        if (_heat > GetMaxHeat() && !_overheated)
        {
            Overheat();
        }
    }

    [Server]
    public void RemoveHeat(float amount)
    {
        if (_heat == 0)
        {
            return;
        }

        if (amount <= 0)
        {
            Debug.LogError("removed 0 or negative heat!?");
        }

        _heat = Math.Max(_heat - amount, 0);

        if (_heat == 0 && _overheated)
        {
            Unoverheat();
        }
    }

    [Server]
    private void Overheat()
    {
        speedModifiers.AddMultiplier(0.25f);
        _overheated = true;
    }

    [Server]
    private void Unoverheat()
    {
        speedModifiers.RemoveMultiplier(0.25f);
        _overheated = false;
    }

    public bool IsOverheated()
    {
        return _overheated;
    }
    #endregion Heat

    public void RaiseOnHitEvent()
    {
    }
}
