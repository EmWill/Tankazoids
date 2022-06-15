using FishNet;
using FishNet.Component.Spawning;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapManager : MonoBehaviour
{
    public struct Player
    {
        public NetworkConnection connection;

        // nullable!
        public Tank tank;

        public Player(NetworkConnection connection, Tank tank)
        {
            this.connection = connection;
            this.tank = tank;
        }
    }

    [SerializeField]
    private NetworkObject _tankPrefab;

    [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
    [SerializeField]
    private bool _addToDefaultScene = true;

    private Dictionary<NetworkConnection, Player> _players;
    public List<Transform> _spawnPoints;

    private NetworkManager _networkManager;

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
        _players.Add(connection, newPlayer);

        SpawnTankForPlayer(newPlayer, furthestPoint(newPlayer));
    }

    [Server]
    public void SpawnTankForPlayer(Player player, Transform location) //asServer bool? umm
    {
        NetworkObject newTank = Instantiate(_tankPrefab, location.position, Quaternion.identity);
        _networkManager.ServerManager.Spawn(newTank, player.connection);

        player.tank = newTank.GetComponent<Tank>();
        player.tank.setMapManager(this);

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
        Transform bestLocation = _spawnPoints[0];

        foreach(Transform spawnPoint in _spawnPoints)
        {
            float currMaxDistance = -1;
            foreach (Player player in _players.Values)
            {
                if (player.tank == null || player.Equals(targetPlayer))
                {
                    continue;
                }

                if (currMaxDistance < 0)
                {
                    currMaxDistance = (player.tank.transform.position - spawnPoint.position).magnitude;
                }
                else
                {
                    float newDistance = (player.tank.transform.position - spawnPoint.position).magnitude;
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
        tank.Despawn();
        Destroy(tank.gameObject);
    }

    public void Respawn(Tank tank)
    {
        DespawnTank(tank);
        NetworkConnection connection = tank.Owner;

        Player tankPlayer = _players[connection];
        SpawnTankForPlayer(tankPlayer, furthestPoint(tankPlayer));
    }


}
