using System;
using UnityEngine;



public class BoardTile : MonoBehaviour
{
    [SerializeField] Vector2Int position;
    [SerializeField] TileOwnerType currentOwner = TileOwnerType.Empty;


    public void OnMouseDown()
    {
        //Debug.Log("Clicked tile "+position.ToString());
        GameEvents.OnClickTile?.Invoke(this);
    }

    public void SetCurrentOwner(TileOwnerType state)
    {
        currentOwner = state;
    }

    public TileOwnerType GetCurrentOwner() => currentOwner;

    public Vector2Int GetPosition() => position;

}
