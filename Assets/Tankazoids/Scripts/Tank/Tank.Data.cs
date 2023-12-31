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


public partial class Tank : NetworkBehaviour
{
    public struct InputData
    {
        public readonly Vector3 worldTargetPos;

        public readonly Vector2 directionalInput;

        public InputStatus weapon0Button;
        public InputStatus weapon1Button;
        public InputStatus bodyButton;
        public InputStatus treadsButton;
        public InputStatus sprintButton;
        public InputStatus swapButton;

        public InputData(Vector3 worldTargetPos, Vector2 directionalInput, InputStatus weapon0Button, InputStatus weapon1Button,
            InputStatus bodyButton, InputStatus treadsButton, InputStatus sprintButton, InputStatus swapButton)
        {
            this.worldTargetPos = worldTargetPos;

            this.directionalInput = directionalInput;

            this.weapon0Button = weapon0Button;
            this.weapon1Button = weapon1Button;
            this.bodyButton = bodyButton;
            this.treadsButton = treadsButton;
            this.sprintButton = sprintButton;
            this.swapButton = swapButton;
        }
    }

    public struct ReconcileData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Quaternion weaponRotation;

        public Vector3 rigidbodyVelocity;
        public float rigidbodyAngularVelocity;

        public float heat;

        // todo this is kinda icky i should figure out why it wont auto serialize statmods
        public float speedModifierMultiplier;
        public float speedModifierBonus;

        public bool knockedBack;

        public byte[] weapon0ReconcileData;
        public byte[] weapon1ReconcileData;
        public byte[] bodyReconcileData;
        public byte[] treadsReconcileData;

        public ReconcileData(Vector3 position, Quaternion rotation, Quaternion weaponRotation,
            Vector3 rigidbodyVelocity,
            float rigidbodyAngularVelocity,
            float heat,
            StatManager speedModifiers,
            bool knockedBack,
            byte[] weapon0ReconcileData,
            byte[] weapon1ReconcileData,
            byte[] bodyReconcileData,
            byte[] treadsReconcileData
            )
        {
            this.position = position;
            this.rotation = rotation;

            this.rigidbodyVelocity = rigidbodyVelocity;
            this.rigidbodyAngularVelocity = rigidbodyAngularVelocity;

            this.heat = heat;
            this.speedModifierMultiplier = speedModifiers.Multiplier;
            this.speedModifierBonus = speedModifiers.Bonus;

            this.knockedBack = knockedBack;

            this.weaponRotation = weaponRotation;

            this.weapon0ReconcileData = weapon0ReconcileData;
            this.weapon1ReconcileData = weapon1ReconcileData;
            this.bodyReconcileData = bodyReconcileData;
            this.treadsReconcileData = treadsReconcileData;
        }
    }

    private InputData GetInputData()
    {
        // todo get a reference to the camera... maybe
        Camera camera = Camera.main;

        // todo this is bad?
        if (camera == null)
        {
            return default;
        }

        Vector3 mousePosition = Input.mousePosition;
        Vector3 worldPointCoords = camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -camera.transform.position.z));
        Vector3 mouseWorldCoords = new(worldPointCoords.x, worldPointCoords.y, 0);

        return new InputData(
                mouseWorldCoords,
                new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
                GetButtonData("Weapon0"),
                GetButtonData("Weapon1"),
                GetButtonData("Body"),
                GetButtonData("Tread"),
                GetButtonData("Sprint"),
                GetButtonData("Swap")
            );
    }

    // this is only a little icky
    private readonly Dictionary<String, bool> _lastInput = new();

    private InputStatus GetButtonData(String name)
    {
        bool found = _lastInput.TryGetValue(name, out bool wasButtonPressed);

        if (!found)
        {
            wasButtonPressed = false;
        }

        bool isButtonPressed = Input.GetButton(name);
        _lastInput[name] = isButtonPressed;
        if (isButtonPressed)
        {
            if (wasButtonPressed)
            {
                return InputStatus.BUTTON_HELD;
            } else
            {
                return InputStatus.BUTTON_DOWN;
            }
        } else
        {
            if (wasButtonPressed)
            {
                return InputStatus.BUTTON_UP;
            } else
            {
                return InputStatus.NONE; 
            }
        }
    }

    private ReconcileData GetReconcileData()
    {
        // we could save on this allocation if we wanted to make the interface less nice... maybe we do
        Writer weapon0Writer = new();
        Writer weapon1Writer = new();
        Writer bodyWriter = new();
        Writer treadsWriter = new();

        // ask the components to write their data to the writers
        _weapon0Component.GetReconcileData(weapon0Writer);
        _weapon1Component.GetReconcileData(weapon1Writer);
        _bodyComponent.GetReconcileData(bodyWriter);
        _treadsComponent.GetReconcileData(treadsWriter);

        ReconcileData reconcileData = new ReconcileData(
            transform.position,
            transform.rotation,
            weaponContainer.transform.rotation,
            rigidbody2d.velocity,
            rigidbody2d.angularVelocity,
            _heat,
            speedModifiers,
            _knockedBack,

            weapon0Writer.GetArraySegment().Array,
            weapon1Writer.GetArraySegment().Array,
            bodyWriter.GetArraySegment().Array,
            treadsWriter.GetArraySegment().Array
            );

        return reconcileData;
    }
}
