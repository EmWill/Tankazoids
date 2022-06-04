using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tank : NetworkBehaviour
{
    public const float BaseMaxHealth = 100f;
    public const float BaseMoveSpeed = 1f;
    public const float BaseRateOfFire = .3f;
    public const int BaseMaxAmmo = 20;
    public const float BaseMaxHeat = 100f;

    public StatManager<float> MaxHealth { get; private set; }
    public StatManager<float> MoveSpeed { get; private set; }
    public StatManager<float> RateOfFire { get; private set; }
    public StatManager<int> MaxAmmo { get; private set; }
    public StatManager<float> MaxHeat { get; private set; }
    public StatManager<float> Damage { get; private set; }
    public StatManager<float> Speed { get; private set; }

    private int _ammo;
    private float _heat;
    private float _health;
    
    [SerializeField]
    private NetworkObject _bullet;

    public delegate void OnHitHandler(Bullet bullet);
    public event OnHitHandler OnHit;

    private void Awake()
    {
        MaxHealth = new StatManager<float>(BaseMaxHealth);
        _health = BaseMaxHealth;
        MoveSpeed = new StatManager<float>(BaseMoveSpeed);
        RateOfFire = new StatManager<float>(BaseRateOfFire);
        MaxAmmo = new StatManager<int>(BaseMaxAmmo);
        MaxHeat = new StatManager<float>(BaseMaxHeat);
        _heat = 0;
    }

    public void RaiseOnHitEvent(Bullet bullet)
    {
        OnHit(bullet);
    }
}
