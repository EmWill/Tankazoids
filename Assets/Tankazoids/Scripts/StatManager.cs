using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StatManager<T>
{
    private readonly List<StatModifier<T>> _statModifiers;
    private readonly Dictionary<T, T> _cache;

    public StatManager()
    {
        _statModifiers = new List<StatModifier<T>>();
        _cache = new();
    }

    public void AddStatModifier(StatModifier<T> mod)
    {
        _statModifiers.Add(mod);
        _statModifiers.Sort();

        // invalidate cache
        _cache.Clear();
    }

    public void RemoveStatModifier(StatModifier<T> mod)
    {
        _statModifiers.Remove(mod);

        // invalidate cache
        _cache.Clear();
    }

    private T CalculateStat(T value)
    {
        if (_cache.ContainsKey(value))
        {
            return _cache[value];
        }

        T resultStat = value;
        foreach (StatModifier<T> mod in _statModifiers)
        {
            resultStat = mod.Mod(resultStat);
        }

        _cache.Add(value, resultStat);
        return resultStat;
    }
}

