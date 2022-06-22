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
    [ServerRpc]
    public void SwapWeapons()
    {
        print("we swapping no stopping");
        GameObject leftHand = _weapon0Object;
        EquipWeapon0(_weapon1Object);
        EquipWeapon1(leftHand);
    }

    // todo mkae this server_spawnweapon and put equip elsewhere?
    public void EquipWeapon0(GameObject prefab)
    {
        GameObject oldWeapon = _weapon0Object;
        NetworkBehaviour oldWeaponComponent = _weapon0Component;


        _weapon0Object = Instantiate(prefab, weaponContainer.transform);
        InstanceFinder.ServerManager.Spawn(_weapon0Object.GetComponent<NetworkObject>(), base.Owner);

        _weapon0Component = _weapon0Object.GetComponent<AbstractWeapon>();
        _weapon0Component.OnEquip(this);
        UpdateClientWeapon0(base.Owner, _weapon0Object, _weapon0Component);


        if (oldWeapon == null)
        {
            return;
        }

        oldWeaponComponent.Despawn();
        Destroy(oldWeapon);
    }

    public void EquipWeapon1(GameObject prefab)
    {
        GameObject oldWeapon = _weapon1Object;
        NetworkBehaviour oldWeaponComponent = _weapon1Component;


        _weapon1Object = Instantiate(prefab, weaponContainer.transform);
        _weapon1Object.transform.localScale = new Vector3(_weapon1Object.transform.localScale.x, _weapon1Object.transform.localScale.y, -_weapon1Object.transform.localScale.z);
        InstanceFinder.ServerManager.Spawn(_weapon1Object.GetComponent<NetworkObject>(), base.Owner);

        _weapon1Component = _weapon1Object.GetComponent<AbstractWeapon>();
        _weapon1Component.OnEquip(this);
        UpdateClientWeapon1(base.Owner, _weapon1Object, _weapon1Component);


        if (oldWeapon == null)
        {
            return;
        }

        oldWeaponComponent.Despawn();
        Destroy(oldWeapon);
    }

    public void EquipBody(GameObject prefab)
    {
        GameObject oldBody = _bodyObject;
        NetworkBehaviour oldBodyComponent = _bodyComponent;

        _bodyObject = Instantiate(prefab, bodyContainer.transform);
        InstanceFinder.ServerManager.Spawn(_bodyObject.GetComponent<NetworkObject>(), base.Owner);

        _bodyComponent = _bodyObject.GetComponent<AbstractBody>();
        _bodyComponent.OnEquip(this);
        UpdateClientBody(base.Owner, _bodyObject, _bodyComponent);

        if (oldBody == null)
        {
            return;
        }

        oldBodyComponent.Despawn();
        Destroy(oldBody);
    }

    public void EquipTread(GameObject prefab)
    {
        GameObject oldTread = _treadObject;
        NetworkBehaviour oldTreadComponent = _treadsComponent;

        _treadObject = Instantiate(prefab, treadContainer.transform);
        InstanceFinder.ServerManager.Spawn(_treadObject.GetComponent<NetworkObject>(), base.Owner);

        _treadsComponent = _treadObject.GetComponent<AbstractTread>();
        _treadsComponent.OnEquip(this);
        UpdateClientTread(_treadObject, _treadsComponent);

        if (oldTread == null)
        {
            return;
        }

        oldTreadComponent.Despawn();
        Destroy(oldTread);
    }

    [ObserversRpc(BufferLast = true)]
    public void UpdateClientWeapon0(NetworkConnection conn, GameObject weaponObject, AbstractWeapon weaponComponent)
    {
        weaponObject.transform.SetParent(weaponContainer.transform);

        _weapon0Object = weaponObject;
        _weapon0Component = weaponComponent;

        _weapon0Component.OnEquip(this);
    }

    [ObserversRpc(BufferLast = true)]
    public void UpdateClientWeapon1(NetworkConnection conn, GameObject weaponObject, AbstractWeapon weaponComponent)
    {
        weaponObject.transform.SetParent(weaponContainer.transform);

        _weapon1Object = weaponObject;
        _weapon1Component = weaponComponent;

        _weapon1Component.OnEquip(this);
    }

    [ObserversRpc(BufferLast = true)]
    public void UpdateClientBody(NetworkConnection conn, GameObject bodyObject, AbstractBody bodyComponent)
    {
        bodyObject.transform.SetParent(bodyContainer.transform);

        _bodyObject = bodyObject;
        _bodyComponent = bodyComponent;

        _bodyComponent.OnEquip(this);
    }

    [ObserversRpc(BufferLast = true)]
    public void UpdateClientTread(GameObject treadObject, AbstractTread treadComponent)
    {
        treadObject.transform.SetParent(treadContainer.transform);

        _treadObject = treadObject;
        _treadsComponent = treadComponent;

        _treadsComponent.OnEquip(this);
    }
}
