using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeatDisplay : MonoBehaviour
{
    public TankUIManager tankUIManager;

    private TextMeshProUGUI _textComponent;

    private void Awake()
    {
        _textComponent = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (tankUIManager.tank == null) return;
        _textComponent.SetText($"{tankUIManager.tank.GetHeat():0.00}% / {tankUIManager.tank.GetMaxHeat():0.00}%");
    }
}
