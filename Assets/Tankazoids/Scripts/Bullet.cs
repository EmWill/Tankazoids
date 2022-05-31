using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float Damage { get; set; }
    public float Speed { get; set; }

    private void Awake()
    {
        Damage = 10;
        Speed = 500;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * Speed);
    }
}
