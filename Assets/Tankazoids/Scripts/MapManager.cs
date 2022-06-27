using FishNet;
using FishNet.Component.Spawning;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public NetworkConnection connection;

    // nullable!
    private Tank tank;

    private string name;

    public Player()
    {
        //Debug.LogError("DON'T EVER USER DEFAULT PLAYER CONSTRUCTOR");
    }

    public Player(NetworkConnection connection, Tank tank)
    {
        this.connection = connection;
        SetTank(tank);
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string finalName = "";
        for (int i = 0; i < 5; i++)
        {
            finalName += chars[Random.Range(0, chars.Length)];
        }
        name = finalName;
    }

    public string GetName()
    {
        return this.name;
    }

    public Tank GetTank()
    {
        return this.tank;
    }

    public void SetTank(Tank tank)
    {
        if (this.tank == tank)
        {
            return;
        }
        this.tank = tank;
        tank.SetPlayer(this);
    }
}

public class MapManager : MonoBehaviour
{

    [SerializeField]
    private NetworkObject _tankPrefab;

    [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
    [SerializeField]
    private bool _addToDefaultScene = true;

    private Dictionary<NetworkConnection, Player> _players;
    public List<Transform> _spawnPoints;

    private NetworkManager _networkManager;
    public GameMode _gameMode;

    private void Start()
    {
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null)
        {
            Debug.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }

        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
            _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
    }

    /// <summary>
    /// Called when a client loads initial scenes after connecting.
    /// </summary>
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection connection, bool asServer)
    {
        if (!asServer)
            return;
        if (_tankPrefab == null)
        {
            Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {connection.ClientId}.");
            return;
        }

        Player newPlayer = new Player(connection, null);
        _gameMode.AddPlayer(newPlayer);
        _players.Add(connection, newPlayer);

        SpawnTankForPlayer(newPlayer, furthestPoint(newPlayer));
    }

    public void SpawnTankForPlayer(Player player, Transform location) //asServer bool? umm
    {
        NetworkObject newTank = Instantiate(_tankPrefab, location.position, Quaternion.identity);
        _networkManager.ServerManager.Spawn(newTank, player.connection);
        player.SetTank(newTank.GetComponent<Tank>());
        player.GetTank().setMapManager(this);

        //If there are no global scenes 
        if (_addToDefaultScene)
            _networkManager.SceneManager.AddOwnerToDefaultScene(newTank);
    }

    private void Awake()
    {
        _players = new Dictionary<NetworkConnection, Player>();
    }

    private Transform furthestPoint(Player targetPlayer)
    {
        float maxDistance = -1;
        int randomSpawnIndex = Random.Range(0, _spawnPoints.Count);
        Transform bestLocation = _spawnPoints[randomSpawnIndex];


        float initialMaxDistance = -1;
        foreach (Player player in _players.Values)
        {
            Tank tank = player.GetTank();
            if (tank == null || player.Equals(targetPlayer))
            {
                continue;
            }

            if (initialMaxDistance < 0)
            {
                initialMaxDistance = (tank.transform.position - bestLocation.position).magnitude;
            }
            else
            {
                float newDistance = (tank.transform.position - bestLocation.position).magnitude;
                if (newDistance > initialMaxDistance)
                {
                    initialMaxDistance = newDistance;
                }
            }
        }
        if (initialMaxDistance < 0) return bestLocation; // THERE'S NO PLAYERS ON THE MAP! GO RANDOM!
            maxDistance = initialMaxDistance;

        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            if (i == randomSpawnIndex)
            {
                continue;
            }
            Transform spawnPoint = _spawnPoints[i];
            float currMaxDistance = -1;
            foreach (Player player in _players.Values)
            {
                Tank tank = player.GetTank();
                if (tank == null || player.Equals(targetPlayer))
                {
                    continue;
                }

                if (currMaxDistance < 0)
                {
                    currMaxDistance = (tank.transform.position - spawnPoint.position).magnitude;
                }
                else
                {
                    float newDistance = (tank.transform.position - spawnPoint.position).magnitude;
                    if (newDistance > currMaxDistance)
                    {
                        currMaxDistance = newDistance;
                    }
                }
            }
            if (maxDistance < 0 || currMaxDistance > maxDistance)
            {
                maxDistance = currMaxDistance;
                bestLocation = spawnPoint;
            }
        }
        return bestLocation;
    }

    public void DespawnTank(Tank tank)
    {
        tank.name = "despawned tank";
        tank.Despawn();
        // Destroy(tank.gameObject);
    }

    public void Respawn(Tank tank)
    {
        DespawnTank(tank);
        NetworkConnection connection = tank.Owner;

        Player tankPlayer = _players[connection];
        SpawnTankForPlayer(tankPlayer, furthestPoint(tankPlayer));
    }


}
