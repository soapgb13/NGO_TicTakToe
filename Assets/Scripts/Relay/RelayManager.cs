using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager instance;
    [SerializeField] UnityTransport transport;
    
    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    ///  Create new relay and return Join Code
    /// </summary>
    /// <param name="numberOfPlayers"> Total number of players in lobby including Host! Doing player count - 1 in relay Allocation </param>
    public async Task<string> CreateRelay(int numberOfPlayers)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(numberOfPlayers - 1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            return joinCode;
        }
        catch (Exception e)
        {
            Debug.LogError(e);

            return "0";
        }
    }

    public async Task<bool> JoinRelay(string joinCode)
    {
        Debug.Log("JoinRelay called with Join code :"+joinCode);

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            Debug.Log($"JoinRelay JoinAllocation id :  {joinAllocation.AllocationId} ");
            
            transport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            Debug.Log($"JoinRelay set client relay data done");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return false;
        }
    }
    
}
