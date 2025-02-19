using System;
using UnityEngine;



public class BoardTile : MonoBehaviour
{
    [SerializeField] Vector2Int position;
    string _currentOwner = "";


    public void OnMouseDown()
    {
        GameEvents.OnClickTile?.Invoke(this);
    }

    public void SetCurrentOwner(string state)
    {
        _currentOwner = state;
    }

    public string GetCurrentOwner() => _currentOwner;

    public Vector2Int GetPosition() => position;

}
