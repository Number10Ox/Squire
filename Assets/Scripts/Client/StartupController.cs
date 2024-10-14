using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartupController : MonoBehaviour
{
    public ushort Port = 7979;
    public string Address = "127.0.0.1";
    
    void Start()
    {
        DestroyLocalSimulationWorld();
        StartServer();
        StartClient();
        SceneManager.LoadScene(1);
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

        // TODO Seems to be done automatically in Editor?
        // var serverEndpoint = NetworkEndpoint.AnyIpv4.WithPort(Port);
        // {
        //     using var networkDriverQuery = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
        //     networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(serverEndpoint);
        // }
    }

    private void StartClient()
    {
        var clientWorld = ClientServerBootstrap.CreateClientWorld("Squire Client World");

        // TODO Seems to be done automatically in Editor?
        // var connectionEndpoint = NetworkEndpoint.Parse(Address, Port);
        // {
        //     using var networkDriverQuery = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
        //     networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, connectionEndpoint);
        // }
            
        World.DefaultGameObjectInjectionWorld = clientWorld;
            
        var gameJoinRequestEntity = clientWorld.EntityManager.CreateEntity();
        clientWorld.EntityManager.AddComponentData(gameJoinRequestEntity, new GameJoinRequest() { });
    }
}
