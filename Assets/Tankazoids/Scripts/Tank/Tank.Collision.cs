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


public partial class Tank : NetworkBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // collision.rigidbody.
    }
}
