using FishNet.Component.Spawning;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapManager : MonoBehaviour
{
    public struct Player
    {
        public NetworkConnection connection;
        public Tank tank;

        public Player(NetworkConnection connection, Tank tank)
        {
            this.connection = connection;
            this.tank = tank;
        }
    }

    private CustomPlayerSpawner _playerSpawner;
    private Dictionary<Tank, Player> _players;
    public List<Transform> _spawnPoints;

    private void Awake()
    {
        _playerSpawner = GetComponent<CustomPlayerSpawner>();
        _players = new Dictionary<Tank, Player>();
    }

    public void addPlayer(NetworkConnection connection, Tank tank)
    {
        print(connection + " and " + tank);
        tank.setMapManager(this);
        _players.Add(tank, new Player(connection, tank));
    }

    private Vector3 furthestPoint(Player self)
    {
        float maxDistance = -1;
        Vector3 bestLocation = _spawnPoints[0].position;
        foreach(Transform spawnPoint in _spawnPoints)
        {
            float currMaxDistance = -1;
            foreach (Player player in _players.Values)
            {
                if (player.connection.Equals(self.connection))
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
                bestLocation = spawnPoint.position;
            }
        }
        return bestLocation;
    }

    public void respawn (Tank tank)
    {
        Player player = _players[tank];
        NetworkConnection connection = player.connection;
        tank.Despawn();
        Destroy(tank);
        _playerSpawner.Respawn(connection, furthestPoint(player));
    }


}
