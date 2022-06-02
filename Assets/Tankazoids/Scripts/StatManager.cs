using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StatManager<T>
{
    public T Value { get; private set; }
    private readonly List<StatModifier<T>> _statModifiers;
    private readonly T _baseStat;

    public StatManager(T baseStat)
    {
        _statModifiers = new List<StatModifier<T>>();
        _baseStat = baseStat;
    }

    public void AddStatModifier(StatModifier<T> mod)
    {
        _statModifiers.Add(mod);
        Value = CalculateStat(_statModifiers, _baseStat);
    }

    public void RemoveStatModifier(StatModifier<T> mod)
    {
        _statModifiers.Remove(mod);
        Value = CalculateStat(_statModifiers, _baseStat);
    }

    private T CalculateStat(List<StatModifier<T>> mods, T initialStat)
    {
        T resultStat = initialStat;
        mods.Sort();
        foreach (StatModifier<T> mod in mods)
        {
            resultStat = mod.Mod(resultStat);
        }
        return resultStat;
    }
}

