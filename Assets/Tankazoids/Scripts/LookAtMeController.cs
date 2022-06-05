using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtMeController : NetworkBehaviour
{
    public GameObject fancyCameraPrefab;
    public override void OnStartClient()
    {
        base.OnStartClient();

        Debug.Log("wubba lubba dub dub");

        if (base.IsOwner)
        {
            GameObject cameraPrefab = Instantiate(fancyCameraPrefab, Vector3.zero, Quaternion.identity);
            cameraPrefab.GetComponent<CameraDirectorController>().followingObject = gameObject;
        }
    }
}
