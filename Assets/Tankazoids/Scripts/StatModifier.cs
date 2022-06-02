using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate T ModifierFunction<T>(T statIn);
public class StatModifier<T> : IComparable<StatModifier<T>>
{
    public ModifierFunction<T> Mod;
    private readonly int _weight;

    public StatModifier(ModifierFunction<T> mod, int weight)
    {
        Mod = mod;
        _weight = weight;
    }

    public StatModifier() : this(default, default) { }
    public int CompareTo(StatModifier<T> other)
    {
        return _weight - other._weight;
    }
}