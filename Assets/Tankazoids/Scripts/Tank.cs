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

    public delegate void OnHitHandler(Bullet bullet);
    public event OnHitHandler OnHit;

    private Rigidbody2D _rigidbody2D;

    public struct InputData
    {
        public float horizontal;
        public float vertical;

        public bool weapon0Pressed;
        public bool weapon1Pressed;
        public bool bodyPressed;
        public bool treadPressed;

        public InputData(float horizontal, float vertical, bool weapon0Pressed, bool weapon1Pressed, bool bodyPressed, bool treadPressed)
        {
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
        public ReconcileData(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
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

    public override void OnStartClient()
    {
        base.OnStartClient();

        Debug.Log("client moment");

        if (base.IsOwner)
        {
            EquipWeapon0(defaultWeapon0Prefab);
            EquipBody(defaultBodyPrefab);
            EquipTread(defaultTreadPrefab);
        }
    }

    private void OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);
            HandleMovement(GetInputData(), false);
        }

        if (base.IsServer)
        {
            HandleMovement(default, true);
            ReconcileData reconcileData = new ReconcileData(transform.position, transform.rotation);
            Reconciliation(reconcileData, true);
        }
    }

    private InputData GetInputData()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal == 0f && vertical == 0f)
            return default;

        return new InputData(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"),
                Input.GetButton("Weapon0"),
                Input.GetButton("Weapon1"),
                Input.GetButton("Body"),
                Input.GetButton("Tread")
            );
    }

    [Replicate]
    private void HandleMovement(InputData inputData, bool asServer, bool replaying = false)
    {
        if (_treadComponent == null)
        {
            return;
        }
        _rigidbody2D.MovePosition(_treadComponent.HandleMovement(new Vector2(inputData.horizontal, inputData.vertical), inputData.treadPressed, transform.position, this));
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer)
    {
        transform.position = rd.position;
        transform.rotation = rd.rotation;
    }

    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
        {
            InstanceFinder.TimeManager.OnTick -= OnTick;
        }
    }

    [ServerRpc]
    public void EquipWeapon0(GameObject prefab) {
        GameObject oldWeapon = _weapon0Object;
        NetworkBehaviour oldWeaponComponent = _weapon0Component;


        _weapon0Object = Instantiate(prefab, weaponContainer.transform);
        InstanceFinder.ServerManager.Spawn(_weapon0Object.GetComponent<NetworkObject>(), base.Owner);

        Debug.Log("weapon");
        UpdateClientWeapon0(base.Owner, _weapon0Object, _weapon0Component);

        if (oldWeapon == null)
        {
            return;
        }

        oldWeaponComponent.Despawn();
        Destroy(oldWeapon);
    }

    [TargetRpc]
    public void UpdateClientWeapon0(NetworkConnection conn, GameObject weaponObject, AbstractWeapon weaponComponent)
    {
        _weapon0Object = weaponObject;
        _weapon0Component = weaponComponent;
    }

    [ServerRpc]
    public void EquipBody(GameObject prefab)
    {
        GameObject oldBody = _bodyObject;
        NetworkBehaviour oldBodyComponent = _bodyComponent;

        _bodyObject = Instantiate(prefab, bodyContainer.transform);
        InstanceFinder.ServerManager.Spawn(_bodyObject.GetComponent<NetworkObject>(), base.Owner);

        _bodyComponent = _bodyObject.GetComponent<AbstractBody>();
        Debug.Log("body");
        UpdateClientBody(base.Owner, _bodyObject, _bodyComponent);

        if (oldBody == null)
        {
            return;
        }

        oldBodyComponent.Despawn();
        Destroy(oldBody);
    }

    [TargetRpc]
    public void UpdateClientBody(NetworkConnection conn, GameObject bodyObject, AbstractBody bodyComponent)
    {
        _bodyObject = bodyObject;
        _bodyComponent = bodyComponent;
    }

    [ServerRpc]
    public void EquipTread(GameObject prefab)
    {
        GameObject oldTread = _treadObject;
        NetworkBehaviour oldTreadComponent = _treadComponent;

        _treadObject = Instantiate(prefab, treadContainer.transform);
        InstanceFinder.ServerManager.Spawn(_treadObject.GetComponent<NetworkObject>(), base.Owner);

        _treadComponent = _treadObject.GetComponent<AbstractTread>();
        Debug.Log("tread");
        UpdateClientTread(base.Owner, _treadObject, _treadComponent);

        if (oldTread == null)
        {
            return;
        }

        oldTreadComponent.Despawn();
        Destroy(oldTread);
    }

    [TargetRpc]
    public void UpdateClientTread(NetworkConnection conn, GameObject treadObject, AbstractTread treadComponent)
    {
        _treadObject = treadObject;
        _treadComponent = treadComponent;
    }

    public void RaiseOnHitEvent(Bullet bullet)
    {
        OnHit(bullet);
    }
}
