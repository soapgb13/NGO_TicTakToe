using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkSpawner : MonoBehaviour
{
    [SerializeField] private GameObject NetworkPrefab;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GlobalUI globalUI;
    private void Awake()
    {
        if (NetworkManager.Singleton == null)
        {
            Instantiate(NetworkPrefab);
        }

        if (GameManager.instance == null)
        {
           Instantiate(gameManager);
        }

        if (GlobalUI.Singleton == null)
        {
            Instantiate(globalUI);
        }
    }
}
