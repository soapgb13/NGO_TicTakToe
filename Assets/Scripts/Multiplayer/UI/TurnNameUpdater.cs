using System;
using TMPro;
using UnityEngine;

public class TurnNameUpdater : MonoBehaviour
{

    public TMP_Text nameText;

    private void OnEnable()
    {
        GameEvents.OnCurrentTurnUpdated += UpdateName;
    }

    private void OnDestroy()
    {
        GameEvents.OnCurrentTurnUpdated -= UpdateName;
    }

    private void UpdateName(string owner)
    {
        nameText.text = GameManager.instance.GetNameFromID(owner);
    }
}
