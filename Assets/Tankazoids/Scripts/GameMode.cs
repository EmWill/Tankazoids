using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class GameMode : NetworkBehaviour
{
    [SyncObject]
    private readonly SyncList<Standing> _leaderboard = new SyncList<Standing>();
    private TextMeshProUGUI _textGUI;

    private void Awake()
    {
        _textGUI = GetComponent<TextMeshProUGUI>();
        _leaderboard.OnChange += _leaderboard_OnChange;
    }

    public class Standing
    {
        public Standing()
        {
            Debug.LogError("DON'T USE THIS EVER");
        }
        public Standing(Player player)
        {
            _player = player;
            _score = 0f;
        }
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
        foreach (Standing currStanding in _leaderboard)
        {
            finalText += currStanding._player.GetName() + ": " + currStanding._score + "\n";
        }
        _textGUI.SetText(finalText);
    }

    public void UpdateScore(Player player, float addition)
    {
        Standing standing = GetStanding(player);
        if (standing == null)
        {
            Debug.LogWarning("you are trying to update the score of a player who isn't on the damn list");
            return;
        }
        standing._score += addition;

        int standingIndex = 0;
        int toSwap = -1;
        bool prior = true;
        for (int i = 0; i < _leaderboard.Count; i++)
        {
            if (standing == _leaderboard[i]) {
                prior = false;
                standingIndex = i;
                if (toSwap == -1) { toSwap = standingIndex; }
                continue;
            }
            if (prior && standing._score < _leaderboard[i]._score)
            {
                toSwap = i;
            }
            else if (!prior && standing._score > _leaderboard[i]._score)
            {

            }
        }
        if (toSwap != standingIndex)
        {
            Standing standingToSwap = _leaderboard[toSwap];
            _leaderboard[toSwap] = standing;
            _leaderboard[standingIndex] = standingToSwap;
        }

        UpdateBoard();
    }


}