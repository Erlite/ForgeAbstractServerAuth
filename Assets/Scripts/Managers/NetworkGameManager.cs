using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using Sburb.Components;
using Sburb.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace Sburb.Networking
{
    public class NetworkGameManager : NetworkGameManagerBehavior
    {
        [SerializeField]
        public static Dictionary<uint, NetworkControlledPlayer> players;

        [SerializeField]
        private Dictionary<uint, NetworkInputController> inputControllers;
        [SerializeField]
        private SpawnManager spawnManager;

        void Start()
        {
            inputControllers = new Dictionary<uint, NetworkInputController>();
            players = new Dictionary<uint, NetworkControlledPlayer>();
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();
            // If we're on the server
            if (networkObject.IsServer)
            {
                // Create a new SpawnManager
                spawnManager = NetworkManager.Instance.InstantiateSpawnManager() as SpawnManager;
                spawnManager.networkStarted += (behavior) =>
                {
                    MainThreadManager.Run(() =>
                    {
                        spawnManager.playerInstantiated += OnPlayerCreated;
                    });
                };

                // Spawn the host as a server
                MainThreadManager.Run(() =>
                {
                    // Spawn an input controller for the player.
                    NetworkedInputControllerBehavior inputController = NetworkManager.Instance.InstantiateNetworkedInputController();
                    inputControllers.Add(networkObject.MyPlayerId, inputController as NetworkInputController);
                    // Spawn the player itself.
                    spawnManager.networkObject.SendRpc(SpawnManagerBehavior.RPC_SPAWN_PLAYER, Receivers.Server, networkObject.MyPlayerId, new Vector3(0, 1, 0));

                    inputController.networkStarted += (behavior) =>
                    {
                        MainThreadManager.Run(() =>
                        {
                            inputController.networkObject.ownerId = networkObject.MyPlayerId;
                        });
                    };
                });

                // If a player connects
                NetworkManager.Instance.Networker.playerConnected += (ply, netWorker) =>
                {
                    MainThreadManager.Run(() =>
                    {
                        // Spawn an input controller for the player.
                        NetworkedInputControllerBehavior inputController = NetworkManager.Instance.InstantiateNetworkedInputController();
                        inputControllers.Add(ply.NetworkId, inputController as NetworkInputController);

                        // Spawn the player itself.
                        spawnManager.networkObject.SendRpc(SpawnManagerBehavior.RPC_SPAWN_PLAYER, Receivers.Server, ply.NetworkId, new Vector3(0, 1, 0));

                        inputController.networkStarted += (behavior) =>
                        {
                            MainThreadManager.Run(() =>
                            {
                                inputController.networkObject.ownerId = ply.NetworkId;
                            });
                        };
                    });
                };
            }
        }

        void OnPlayerCreated(uint networkId, NetworkControlledPlayer player)
        {
            Debug.Log("NetworkGameManager => Player spawned and added to dictionary: " + networkId);
            players.Add(networkId, player);
            inputControllers[networkId].networkObject.SendRpc(NetworkInputController.RPC_SET_ACTIVE_ENTITY, Receivers.All, networkId);
        }

    }
}