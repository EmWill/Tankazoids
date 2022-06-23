using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpPickup : NetworkBehaviour
{
    public float cooldownTime = 15f;
    public GameObject floatyObject;

    private uint? appearAtTick = null;

    public override void OnStartServer()
    {
        base.OnStartServer();

        base.TimeManager.OnTick += OnServerTick;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        base.TimeManager.OnTick -= OnServerTick;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (base.IsServer && appearAtTick == null && other.gameObject.TryGetComponent<Tank>(out Tank tank))
        {
            if (!tank.IsMaxHealth())
            {
                tank.AddHealth(50);
                appearAtTick = base.TimeManager.LocalTick + (uint)Mathf.CeilToInt(cooldownTime / (float)base.TimeManager.TickDelta);

                Dissapear();
            }
        }
    }

    [ObserversRpc(RunLocally = true)]
    private void Dissapear()
    {
        floatyObject.SetActive(false);
    }

    private void OnServerTick()
    {
        if (base.TimeManager.LocalTick >= appearAtTick)
        {
            Appear();
        }
    }

    [ObserversRpc(RunLocally = true)]
    private void Appear()
    {
        floatyObject.SetActive(true);
        appearAtTick = null;
    }
}
