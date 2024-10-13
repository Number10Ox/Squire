using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;

public class StartupController : MonoBehaviour
{
    public ushort Port = 7979;
    public string Address = "127.0.0.1";
    
    void Awake()
    {
        DestroyLocalSimulationWorld();
        StartServer();
        StartClient();
    }
    
    private static void DestroyLocalSimulationWorld()
    {
        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }
    }

    private void StartServer()
    {
        var serverWorld = ClientServerBootstrap.CreateServerWorld("Squire Server World");

        var serverEndpoint = NetworkEndpoint.AnyIpv4.WithPort(Port);
        {
            using var networkDriverQuery = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(serverEndpoint);
        }
    }

    private void StartClient()
    {
        var clientWorld = ClientServerBootstrap.CreateClientWorld("Squire Client World");

        var connectionEndpoint = NetworkEndpoint.Parse(Address, Port);
        {
            using var networkDriverQuery = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, connectionEndpoint);
        }
            
        World.DefaultGameObjectInjectionWorld = clientWorld;
            
         var gameJoinRequestEntity = clientWorld.EntityManager.CreateEntity();
        clientWorld.EntityManager.AddComponentData(gameJoinRequestEntity, new GameJoinRequest() { });
    }
}