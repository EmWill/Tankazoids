using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;



public class GameLauncher : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _ipFieldText;

    public void LaunchGame(bool asServer)
    {
        if (!asServer)
        {
            string address = _ipFieldText.text;
            InstanceFinder.ClientManager.StartConnection(address);
        }
        else
        {
            InstanceFinder.ServerManager.OnServerConnectionState += LoadSceneAndStartClient;
            InstanceFinder.ServerManager.StartConnection();
        }
    }

    private const string SCENE_NAME = "Main2d";
    /* Using SCENE_NAME constant for now, will probably change this to lobby scene when we make it.
     * This case is kind of unique because every other time we are launching scenes e.g. changing maps,
     * we should be able to guarantee that the server already exists so we won't have to do it on an
     * event trigger.
     */
    private void LoadSceneAndStartClient(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            SceneLoadData sld = new(SCENE_NAME);
            sld.ReplaceScenes = ReplaceOption.All;
            InstanceFinder.SceneManager.LoadGlobalScenes(sld);
            InstanceFinder.ClientManager.StartConnection("localhost");
            InstanceFinder.ServerManager.OnServerConnectionState -= LoadSceneAndStartClient;
        }
    }
}
