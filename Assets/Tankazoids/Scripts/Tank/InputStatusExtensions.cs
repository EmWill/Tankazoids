using FishNet;
using FishNet.Component.ColliderRollback;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// you should only use the extension mathods here and not compare these values... this is a little weird but it is faster.
public enum InputStatus
{
    BUTTON_DOWN,
    BUTTON_HELD,
    BUTTON_UP,
    NONE
};
public static class InputStatusExtensions
{
    public static bool IsPressed(this InputStatus status)
    {
        return status == InputStatus.BUTTON_DOWN || status == InputStatus.BUTTON_HELD;
    }

    public static bool IsButtonDown(this InputStatus status)
    {
        return status == InputStatus.BUTTON_DOWN;
    }

    public static bool IsButtonUp(this InputStatus status)
    {
        return status == InputStatus.BUTTON_UP;
    }
}
