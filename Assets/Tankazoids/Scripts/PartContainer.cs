using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartContainer : NetworkBehaviour
{
    public bool HasPart {
        get { return _partObject != null; }
    }
    private GameObject _partObject;
    private GameObject _visualPart;
    private AbstractPart _partComponent;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_partObject != null && collision.gameObject.TryGetComponent<Tank>(out Tank tank) && base.IsServer)
        {
            ReplacePart(tank);
        }
    }

    [ServerRpc]
    private void ReplacePart(Tank tank)
    {
        switch (_partComponent)
        {
            case AbstractTread:
                tank.EquipTread(_partObject);
                break;
            case AbstractBody:
                tank.EquipBody(_partObject);
                break;
            case AbstractWeapon:
                tank.EquipWeapon0(_partObject);
                break;
        }
        InstanceFinder.ServerManager.Despawn(_visualPart);
        Destroy(_visualPart);
        _partObject = null;
        _partComponent = null;
    }

    public void SpawnPart(GameObject part, GameObject visualPart)
    {
        if (base.IsServer && !HasPart && part.TryGetComponent<AbstractPart>(out _partComponent))
        {
            _partObject = part;
            _visualPart = visualPart;
            InstanceFinder.ServerManager.Spawn(_visualPart);
        }
    }
}
