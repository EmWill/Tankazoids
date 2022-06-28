using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionDebugger : NetworkBehaviour
{
    public GameObject obj;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        InstanceFinder.TimeManager.OnTick += OnTick;
    }

    public void OnDestroy()
    {
        InstanceFinder.TimeManager.OnTick -= OnTick;
    }

    private void OnTick()
    {
        GameObject proj = Instantiate(obj, transform.position, Quaternion.identity);
        Destroy(proj, 5);
        // Spawn(proj);
    }
}
