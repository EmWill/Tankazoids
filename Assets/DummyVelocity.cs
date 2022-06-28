using FishNet.Object;
using UnityEngine;

public class DummyVelocity : NetworkBehaviour
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        GetComponent<Rigidbody2D>().velocity = Vector2.right * 1;
    }
}