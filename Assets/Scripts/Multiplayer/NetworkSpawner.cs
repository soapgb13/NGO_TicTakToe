using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkSpawner : MonoBehaviour
{
    [SerializeField] private GameObject NetworkPrefab;
    
    private void Awake()
    {
        if (NetworkManager.Singleton == null)
        {
            Instantiate(NetworkPrefab);
        }
    }
}
