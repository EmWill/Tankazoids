using FishNet.Object.Prediction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTreads : AbstractTread
{
    private Rigidbody2D _tankRigidbody;

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

    public override void OnEquip(Tank tank)
    {
        base.OnEquip(tank);

        _tankRigidbody = tank.gameObject.GetComponent<Rigidbody2D>();
    }

    public override void OnTankTick(Tank.InputData inputData)
    {
        base.OnTankTick(inputData);

        if (base.IsOwner)
        {
            ReconcileMovement(default, false);
            ReplicateMovement(inputData, false);
        }

        if (base.IsServer)
        {
            ReplicateMovement(default, true);
            ReconcileData reconcileData = new ReconcileData(transform.position, transform.rotation);
            ReconcileMovement(reconcileData, true);
        }
    }

    [Replicate]
    private void ReplicateMovement(Tank.InputData inputData, bool asServer, bool replaying = false)
    {
        _tankRigidbody.MovePosition(_tank.transform.position + new Vector3(inputData.directionalInput.x, inputData.directionalInput.y, 0) / 10);
    }

    [Reconcile]
    private void ReconcileMovement(ReconcileData reconcileData, bool asServer)
    {
        transform.position = reconcileData.position;
        transform.rotation = reconcileData.rotation;
    }

    public override float GetCooldown()
    {
        return 0;
    }
}
