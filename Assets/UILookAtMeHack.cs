using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILookAtMeHack : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            GameObject.Find("TankStats").GetComponent<TankUIManager>().tank = GetComponent<Tank>();
        }
    }
}
