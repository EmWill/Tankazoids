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

        public float horizontal;
        public float vertical;

        public bool weapon0Pressed;
        public bool weapon1Pressed;
        public bool bodyPressed;
        public bool treadPressed;

        public InputData(Vector3 worldTargetPos, float horizontal, float vertical, bool weapon0Pressed, bool weapon1Pressed, bool bodyPressed, bool treadPressed)
        {
            this.worldTargetPos = worldTargetPos;

            this.horizontal = horizontal;
            this.vertical = vertical;

            this.weapon0Pressed = weapon0Pressed;
            this.weapon1Pressed = weapon1Pressed;
            this.bodyPressed = bodyPressed;
            this.treadPressed = treadPressed;


        }
    }

    public struct ReconcileData
    {
        public Vector3 position;
        public Quaternion rotation;

        public Quaternion weaponContianerRotation;

        public ReconcileData(Vector3 position, Quaternion rotation, Quaternion weaponContianerRotation)
        {
            this.position = position;
            this.rotation = rotation;

            this.weaponContianerRotation = weaponContianerRotation;
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

    private void OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);
            //InputData input = GetInputData();
            Replicate(GetInputData(), false);
            //NonReplicatedInput(input);
        }

        if (base.IsServer)
        {
            Replicate(default, true);
            ReconcileData reconcileData = new ReconcileData(transform.position, transform.rotation, weaponContainer.transform.rotation);
            Reconciliation(reconcileData, true);
        }
    }

    private InputData GetInputData()
    {
        // todo get a reference to the camera... maybe
        Camera camera = Camera.main;

        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPointCoords = camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -camera.transform.position.z));
        Vector3 mouseWorldCoords = new(worldPointCoords.x, worldPointCoords.y, 0);

        return new InputData(
                mouseWorldCoords,
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"),
                Input.GetButton("Weapon0"),
                Input.GetButton("Weapon1"),
                Input.GetButton("Body"),
                Input.GetButton("Tread")
            );
    }

    [Replicate]
    private void Replicate(InputData inputData, bool asServer, bool replaying = false)
    {
        // todo fix this is bad
        RotateWeaponContainerTowardsCoordinates(inputData.worldTargetPos);
        if (base.IsClient)
        {
            Debug.Log("Client:" + inputData.horizontal);
        }
        if (base.IsServer)
        {
            Debug.Log("Server:" + inputData.horizontal);
        }
        _rigidbody2D.MovePosition(_treadComponent.HandleMovement(new Vector2(inputData.horizontal, inputData.vertical), inputData.treadPressed, transform.position, this));
        if (_treadComponent == null)
        {
            return;
        }
    }

    private void NonReplicatedInput(InputData inputData)
    {
        if (inputData.weapon0Pressed)
        {
            _weapon0Component.ActivateAbility(inputData, this);
        }
    }

    private void RotateWeaponContainerTowardsCoordinates(Vector3 coordinates)
    {
        Vector3 difference = coordinates - new Vector3(weaponContainer.transform.position.x, weaponContainer.transform.position.y, 0);
        weaponContainer.transform.eulerAngles = new Vector3(Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg - 90, -90, -90);
    }

    [Reconcile]
    private void Reconciliation(ReconcileData reconcileData, bool asServer)
    {
        transform.position = reconcileData.position;
        transform.rotation = reconcileData.rotation;

        weaponContainer.transform.rotation = reconcileData.weaponContianerRotation;
    }

    public void EquipWeapon0(GameObject prefab) {
        GameObject oldWeapon = _weapon0Object;
        NetworkBehaviour oldWeaponComponent = _weapon0Component;


        _weapon0Object = Instantiate(prefab, weaponContainer.transform);
        InstanceFinder.ServerManager.Spawn(_weapon0Object.GetComponent<NetworkObject>(), base.Owner);

        _weapon0Component = _weapon0Object.GetComponent<AbstractWeapon>();
        UpdateClientWeapon0(base.Owner, _weapon0Object, _weapon0Component);

        if (oldWeapon == null)
        {
            return;
        }

        oldWeaponComponent.Despawn();
        Destroy(oldWeapon);
    }

    [ObserversRpc(BufferLast = true)]
    public void UpdateClientWeapon0(NetworkConnection conn, GameObject weaponObject, AbstractWeapon weaponComponent)
    {
        weaponObject.transform.SetParent(weaponContainer.transform);

        _weapon0Object = weaponObject;
        _weapon0Component = weaponComponent;
    }

    public void EquipBody(GameObject prefab)
    {
        GameObject oldBody = _bodyObject;
        NetworkBehaviour oldBodyComponent = _bodyComponent;

        _bodyObject = Instantiate(prefab, bodyContainer.transform);
        InstanceFinder.ServerManager.Spawn(_bodyObject.GetComponent<NetworkObject>(), base.Owner);

        _bodyComponent = _bodyObject.GetComponent<AbstractBody>();
        UpdateClientBody(base.Owner, _bodyObject, _bodyComponent);

        if (oldBody == null)
        {
            return;
        }

        oldBodyComponent.Despawn();
        Destroy(oldBody);
    }

    [ObserversRpc(BufferLast = true)]
    public void UpdateClientBody(NetworkConnection conn, GameObject bodyObject, AbstractBody bodyComponent)
    {
        bodyObject.transform.SetParent(bodyContainer.transform);

        _bodyObject = bodyObject;
        _bodyComponent = bodyComponent;
    }

    public void EquipTread(GameObject prefab)
    {
        GameObject oldTread = _treadObject;
        NetworkBehaviour oldTreadComponent = _treadComponent;

        _treadObject = Instantiate(prefab, treadContainer.transform);
        InstanceFinder.ServerManager.Spawn(_treadObject.GetComponent<NetworkObject>(), base.Owner);

        _treadComponent = _treadObject.GetComponent<AbstractTread>();
        UpdateClientTread(_treadObject, _treadComponent);

        if (oldTread == null)
        {
            return;
        }

        oldTreadComponent.Despawn();
        Destroy(oldTread);
    }

    [ObserversRpc(BufferLast = true)]
    public void UpdateClientTread(GameObject treadObject, AbstractTread treadComponent)
    {
        treadObject.transform.SetParent(treadContainer.transform);

        _treadObject = treadObject;
        _treadComponent = treadComponent;
    }

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
