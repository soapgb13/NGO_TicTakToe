using System;
using TMPro;
using UnityEngine;

public class TurnNameUpdater : MonoBehaviour
{

    public TMP_Text nameText;

    private void OnEnable()
    {
        BoardManager.OnCurrentTurnUpdated += UpdateName;
        Debug.Log($"TurnNameUpdater subscribe to OnCurrentTurnUpdated!");
    }

    private void OnDestroy()
    {
        BoardManager.OnCurrentTurnUpdated -= UpdateName;
    }

    private void UpdateName(TileOwnerType owner)
    {
        Debug.Log($"TurnNameUpdater UpdateName "+owner);
        nameText.text = owner.ToString();
    }
}
