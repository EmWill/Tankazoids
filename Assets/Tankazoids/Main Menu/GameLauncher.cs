using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Transporting.Tugboat;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour
{
    [SerializeField]
    private NetworkManager _networkManager;
    [SerializeField]
    private TextMeshProUGUI _ipFieldText;

    public void LaunchGame(bool asServer)
    {
        if (!asServer)
        {
            string address = _ipFieldText.text;
            _networkManager.ClientManager.StartConnection(address);
        }
        else
        {
            _networkManager.ClientManager.StartConnection("localhost");
            _networkManager.ServerManager.StartConnection();
        }
        SceneLoadData sld = new SceneLoadData("Main2d");
        sld.ReplaceScenes = ReplaceOption.All;
        _networkManager.SceneManager.LoadGlobalScenes(sld);
    }
}
