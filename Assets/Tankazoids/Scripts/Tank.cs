using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
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
    private AbstractTread _treadComponent;

    public StatManager<float> maxHealthModifiers { get; private set; }
    public StatManager<float> moveSpeedModifiers { get; private set; }
    public StatManager<float> cooldownModifiers { get; private set; }
    public StatManager<int> maxAmmoModifiers { get; private set; }
    public StatManager<float> maxHeatModifiers { get; private set; }
    public StatManager<float> damageModifiers { get; private set; }
    public StatManager<float> speedModifiers { get; private set; }

    private int _ammo;
    private float _heat;
    private float _health;

    public delegate void OnHitHandler(ref float dmg);
    public event OnHitHandler OnHit;

    private Rigidbody2D _rigidbody2D;

    public struct InputData
    {
        public Vector3 worldTargetPos;

        public Vector2 directionalInput;

        public bool weapon0Pressed;
        public bool weapon1Pressed;
        public bool bodyPressed;
        public bool treadPressed;

        public InputData(Vector3 worldTargetPos, Vector2 directionalInput, bool weapon0Pressed, bool weapon1Pressed, bool bodyPressed, bool treadPressed)
        {
            this.worldTargetPos = worldTargetPos;

            this.directionalInput = directionalInput;

            this.weapon0Pressed = weapon0Pressed;
            this.weapon1Pressed = weapon1Pressed;
            this.bodyPressed = bodyPressed;
            this.treadPressed = treadPressed;
        }
    }

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();

        maxHealthModifiers = new();
        moveSpeedModifiers = new();
        cooldownModifiers = new();
        maxAmmoModifiers = new();
        maxHeatModifiers = new();
        damageModifiers = new();
        speedModifiers = new();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        
        InstanceFinder.TimeManager.OnTick += OnTick;
    }

    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
        {
            InstanceFinder.TimeManager.OnTick -= OnTick;
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        EquipWeapon0(defaultWeapon0Prefab);
        EquipBody(defaultBodyPrefab);
        EquipTread(defaultTreadPrefab);
    }

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
                Input.GetButton("Tread")
            );
    }

    private void OnTick()
    {
        InputData inputData = GetInputData();
        _weapon0Component.OnTankTick(inputData);
        // _weapon1Component.OnTankTick(inputData);
        _bodyComponent.OnTankTick(inputData);
        _treadComponent.OnTankTick(inputData);

        NonReplicatedInput(inputData);

        HandleWeaponRotation(inputData);
    }

    [ServerRpc]
    private void NonReplicatedInput(InputData inputData)
    {
        if (inputData.weapon0Pressed)
        {
            _weapon0Component.ActivateAbility(inputData);
        }
    }

    #region Equipping
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
        NetworkBehaviour oldTreadComponent = _treadComponent;

        _treadObject = Instantiate(prefab, treadContainer.transform);
        InstanceFinder.ServerManager.Spawn(_treadObject.GetComponent<NetworkObject>(), base.Owner);

        _treadComponent = _treadObject.GetComponent<AbstractTread>();
        _treadComponent.OnEquip(this);
        UpdateClientTread(_treadObject, _treadComponent);

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
        _treadComponent = treadComponent;

        _treadComponent.OnEquip(this);
    }
    #endregion Equipping

    #region Weapon Turning

    public struct ReconcileWeaponTurnData
    {
        public Quaternion rotation;

        public ReconcileWeaponTurnData(Quaternion rotation)
        {
            this.rotation = rotation;
        }
    }

    private void HandleWeaponRotation(InputData inputData)
    {
        if (base.IsOwner)
        {
            ReconcileWeaponRotation(default, false);
            ReplicateWeaponRotation(inputData, false);
        }

        if (base.IsServer)
        {
            ReplicateWeaponRotation(default, true);
            ReconcileWeaponRotation(new ReconcileWeaponTurnData(weaponContainer.transform.rotation), true);
        }
    }

    [Replicate]
    private void ReplicateWeaponRotation(Tank.InputData inputData, bool asServer, bool replaying = false)
    {
        Vector3 difference = inputData.worldTargetPos - new Vector3(weaponContainer.transform.position.x, weaponContainer.transform.position.y, 0);
        weaponContainer.transform.eulerAngles = new Vector3(Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg - 90, -90, -90);
    }

    [Reconcile]
    private void ReconcileWeaponRotation(ReconcileWeaponTurnData reconcileData, bool asServer)
    {
        weaponContainer.transform.rotation = reconcileData.rotation;
    }
    #endregion Weapon Turning

    public void RaiseOnHitEvent(ref float dmg)
    {
        OnHit(ref dmg);
        _health -= dmg;
        if (_health <= 0)
        {
            //TODO: u r dead.
        }
    }
}
