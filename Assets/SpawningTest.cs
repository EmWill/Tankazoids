using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningTest : NetworkBehaviour
{
    public GameObject obj;

    public override void OnStartServer()
    {
        base.OnStartServer();
        InstanceFinder.TimeManager.OnTick += OnTick;
    }

    private void OnTick()
    {
        if (InstanceFinder.TimeManager.Tick % 200 != 0) return;

        GameObject proj = Instantiate(obj, transform.position, Quaternion.identity);
        Spawn(proj);
    }
}
