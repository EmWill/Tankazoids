using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class CameraController : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            Camera cam = GetComponent<Camera>();
            cam.enabled = true;
        }
    }
}
