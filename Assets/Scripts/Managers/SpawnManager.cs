using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using Sburb.Components;
using UnityEngine;

namespace Sburb.Managers
{
	public class SpawnManager : SpawnManagerBehavior
	{
		public event System.Action<uint, NetworkControlledPlayer> playerInstantiated;
		private uint entityCount;

		void Start()
		{
			entityCount = 0;
		}

		public GameObject InstantiatePlayer(uint networkId)
		{
			return InstantiatePlayer(networkId, new Vector3(0, 1, 0));
		}

		public GameObject InstantiatePlayer(uint networkId, Vector3 position)
		{
            GameObject obj = null;

            MainThreadManager.Run(() =>
            {
                NetworkControlledPlayer networkControlledPlayer = NetworkManager.Instance.InstantiateNetworkControlledEntity(position: position, sendTransform: true) as NetworkControlledPlayer;
                obj = networkControlledPlayer.gameObject;
                networkControlledPlayer.networkStarted += (behavior) =>
                {
                    MainThreadManager.Run(() =>
                    {
                        // Set player owner id and entity id.
                        networkControlledPlayer.networkObject.SendRpc(NetworkControlledEntityBehavior.RPC_SET_OWNER_ID, Receivers.All, networkId);
                        networkControlledPlayer.networkObject.SendRpc(NetworkControlledEntityBehavior.RPC_SET_ENTITY_ID, Receivers.All, entityCount++);

                        // Set the ID the NCD should use to activate/disable script based on network ownership.
                        obj.GetComponent<NetworkComponentDisabler>().networkObject.SendRpc(NetworkComponentDisablerBehavior.RPC_SET_OWNER_ID, Receivers.All, networkId);
                    });

					playerInstantiated(networkId, networkControlledPlayer);
                };
            });

            return obj;
		}
        // TODO: Do this.
        public GameObject InstantiateItem(int Id, Vector3 position)
		{
			throw new System.NotImplementedException();
		}

		public override void SpawnPlayer(RpcArgs args)
		{
			if (!args.Info.SendingPlayer.IsHost || !networkObject.IsServer)
			{
				throw new System.InvalidOperationException("Error: only the server can instantiate a player!");
			}

			uint networkId = args.GetNext<uint>();
			Vector3 position = args.GetNext<Vector3>();
			InstantiatePlayer(networkId, position);
		}

		public override void SpawnItem(RpcArgs args)
		{
			throw new System.NotImplementedException();
		}
	}
}