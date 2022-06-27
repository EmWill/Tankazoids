using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class GameMode : NetworkBehaviour
{
    [SyncObject]
    protected readonly SyncList<Standing> _leaderboard = new SyncList<Standing>();
    protected TextMeshProUGUI _textGUI;
    protected bool _active = true;
    public float MAX_TIME = 45;

    private void Awake()
    {
        _textGUI = GetComponent<TextMeshProUGUI>();
        _leaderboard.OnChange += _leaderboard_OnChange;
    }

    public class Standing
    {
        public Standing()
        {
            //Debug.LogError("DON'T USE THIS EVER");
        }
        public Standing(Player player)
        {
            _player = player;
            _name = player.GetName();
            _score = 0f;
        }
        public string _name;
        public Player _player;
        public float _score;
    }
    public void AddPlayer(Player player)
    {
        _leaderboard.Add(new Standing(player));
        UpdateBoard();
    }

    private void _leaderboard_OnChange(SyncListOperation op, int index, Standing oldItem, Standing newItem, bool asServer)
    {
            UpdateBoard();
    }

    private Standing GetStanding(Player player)
    {
        foreach(Standing standing in _leaderboard)
        {
            if (player == standing._player)
            {
                return standing;
            }
        }
        return null;
    }

    private void UpdateBoard()
    {
        string finalText = "";
        for (int i = _leaderboard.Count - 1; i >= 0; i--)
        {
            Standing currStanding = _leaderboard[i];
            finalText += currStanding._name + ": " + (int) (currStanding._score * TimeManager.TickDelta) + "\n";
        }
        _textGUI.SetText(finalText);
    }

    public void UpdateScore(Player player, float addition)
    {
        if (!base.IsServer) { return; }
        if (!_active) { return; }
        Standing standing = GetStanding(player);
        if (standing == null)
        {
            Debug.LogWarning("you are trying to update the score of a player who isn't on the damn list");
            return;
        }
        Standing copy = standing;
        copy._score += addition;

        _leaderboard.Remove(standing); // is this slow? these lists are tiny but
        int toSwap = 0;
        for (int i = 0; i < _leaderboard.Count; i++)
        {
            if (copy._score > _leaderboard[i]._score)
            {
                toSwap++;
            }
            else
            {
                break;
            }
        }
        _leaderboard.Insert(toSwap, copy); // is this slow? these lists are tiny but

        if (copy._score * TimeManager.TickDelta >= MAX_TIME)
        {
            EndGame();
        }

        UpdateBoard();
    }

    protected void EndGame()
    {
        _active = false;
        Standing copy = _leaderboard[0];
        copy._name += ": WINNER";
        _leaderboard[0] = copy;
        UpdateBoard();
    }


}
