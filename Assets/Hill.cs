using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hill : NetworkBehaviour
{
    private MapManager _manager;
    private List<Tank> _residents;
    private SpriteRenderer _sr;
    public KotHManager _gameMode;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _residents = new List<Tank>();
        _sr.color = Color.black;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        base.TimeManager.OnTick += OnTick;

    }

    private void OnTick()
    {
        if (base.IsServer)
        {
            if (_residents.Count == 1)
            {
                _gameMode.UpdateScore(_residents[0].GetPlayer(), 1f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Tank player = collision.gameObject.GetComponent<Tank>();
        if (player != null)
        {
            _residents.Add(player);
            updateState();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Tank player = collision.gameObject.GetComponent<Tank>();
        if (player != null)
        {
            _residents.Remove(player);
            updateState();
        }
    }

    private void updateState()
    {
        if (_residents.Count == 0)
        {
            _sr.color = Color.black;
        }
        else if (_residents.Count == 1)
        {
            _sr.color = Color.blue;
            _gameMode.UpdateScore(_residents[0].GetPlayer(), 1f);
        }
        else
        {
            _sr.color = Color.red;
        }
    }

}
