using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthDisplay : MonoBehaviour
{
    public TankUIManager tankUIManager;

    private TextMeshProUGUI _textComponent;

    private void Awake()
    {
        _textComponent = GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        _textComponent.SetText($"{tankUIManager.tank.GetHealth():0.00}% / {tankUIManager.tank.GetMaxHealth():0.00}%");
    }
}
