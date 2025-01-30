using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerPrefabNamer : NetworkBehaviour
{
    private void Start()
    {
        if (IsOwner)
        {
            gameObject.name = "Player : Self";
        }
        else
        {
            gameObject.name = "Player :"+NetworkManager.Singleton.LocalClientId.ToString();
        }
    }
}
