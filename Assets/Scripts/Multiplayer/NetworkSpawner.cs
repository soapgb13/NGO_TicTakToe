using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkSpawner : MonoBehaviour
{
    [SerializeField] private GameObject NetworkPrefab;
    [SerializeField] private GameManager gameManager;
    
    private void Awake()
    {
        if (NetworkManager.Singleton == null)
        {
            Instantiate(NetworkPrefab);
        }

        if (GameManager.instance == null)
        {
           var instance = Instantiate(gameManager);
           // var instanceNetworkObject = instance.GetComponent<NetworkObject>();
           // instanceNetworkObject.Spawn();
        }
    }
}
