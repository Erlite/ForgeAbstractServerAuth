using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using NetControlledEntity = Sburb.Abstractions.NetworkControlledEntityBehavior;
using Sburb.Constructs;
using Sburb.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;

namespace Sburb.Networking
{
	public class NetworkInputController : NetworkedInputControllerBehavior
	{
		public NetControlledEntity activeControlledEntity;

		private bool isReady;
		[SerializeField]
		private uint currentSyncFrame;
		[SerializeField]
		private uint serverSyncRate = 5;
        [SerializeField]
        private uint entityFetchRetryAttempts = 10;
        private uint entityFetchAttemptsLeft = 10;
		private InputFrame currentInputFrame;
		private List<InputFrame> inputToPlay;
		private List<InputFrame> inputToSendToServer;
		private List<NetEntityStatus> localStatusHistory;
		private List<NetEntityStatus> authStatusHistory;

		void Start()
		{
			currentSyncFrame = 0;
			localStatusHistory = new List<NetEntityStatus>();
			authStatusHistory = new List<NetEntityStatus>();
			inputToPlay = new List<InputFrame>();
			inputToSendToServer = new List<InputFrame>();
		}

		protected override void NetworkStart()
		{
			base.NetworkStart();

			if (networkObject.ownerId != networkObject.MyPlayerId)
			{
				return;
			}

			gameObject.name = $"NetworkInputController [{networkObject.ownerId}]";
			isReady = true;
			Debug.Log(gameObject.name + " => Ready!");
		}
		
		// Get input if owner and ready
		void Update()
		{
			// If not ready.
			if (!isReady)
			{
				return;
			}

			// If no active controlled entity or the entity isn't active.
			if (activeControlledEntity == null)
			{
				return;
			}

			if (networkObject.ownerId == networkObject.MyPlayerId)
			{
				currentInputFrame = InputFrame.GetInput(currentSyncFrame, activeControlledEntity.transform.rotation);
				inputToSendToServer.Add(currentInputFrame);
			}

			// If the server sync rate has been hit, send local input to server, or if server, send the history of entity statuses for reconciliation.
			if (currentSyncFrame % serverSyncRate == 0)
			{
				// If owner and not server, send input.
				if (networkObject.ownerId == networkObject.MyPlayerId)
				{
					if (!networkObject.IsServer)
					{
						byte[] inputData = ByteHelpers.ObjectToByteArray(inputToSendToServer);
						networkObject.SendRpc(RPC_SYNC_INPUTS, Receivers.Server, inputData);
						inputToSendToServer.Clear();
					}
				}
				// Else if server and not owner, send data for reconciliation.
				else if (networkObject.IsServer && networkObject.ownerId != networkObject.MyPlayerId)
				{
					byte[] history = ByteHelpers.ObjectToByteArray(localStatusHistory);
					networkObject.SendRpc(RPC_SYNC_NET_ENTITY_STATUS_HISTORY, Receivers.All, history, networkObject.ownerId);
					localStatusHistory.Clear();
				}
			}
		}

		// Apply movement on server or owning player
		void FixedUpdate()
		{
			// If not ready or no entity to control, return.
			if (!isReady || activeControlledEntity == null)
			{
				return;
			}

			// If server
			if (networkObject.IsServer)
			{
				// If also owner
				if (networkObject.ownerId == networkObject.MyPlayerId)
				{
					// Perform and send to other clients.
					activeControlledEntity.PerformMovement(currentInputFrame, false);
					activeControlledEntity.SetServerValues();
				}
				// else if not owner
				else
				{
					// If theres input to play sent by the owner, play it and save the entity's status afterwards for reconciliation.
					if (inputToPlay.Any())
					{
						// Get the input frame.
						InputFrame input = inputToPlay.FirstOrDefault();
						// Perform it.
						activeControlledEntity.PerformMovement(input, false);
						// Save the result of where the entity is afterwards and info about it if necessary.
						NetEntityStatus result = activeControlledEntity.GetNetEntityStatus(currentSyncFrame);
						// Add it to the localStatusHistory. For the server, it's just the server history to send to the client.
						localStatusHistory.Add(result);
						// Remove the input we just played.
						inputToPlay.RemoveAt(0);
					}
				}
			}
			// TODO: Check if correct usage of MyPlayerId
			// Else just perform movement and save the result locally.
			else if (networkObject.ownerId == networkObject.MyPlayerId)
			{
				activeControlledEntity.PerformMovement(currentInputFrame, false);
				NetEntityStatus result = activeControlledEntity.GetNetEntityStatus(currentSyncFrame);
				localStatusHistory.Add(result);


				// Check for reconciliation if needed.
				activeControlledEntity.ReconciliateClient(ref localStatusHistory, ref authStatusHistory);
			}

			currentSyncFrame++;
		}

		public override void SetActiveEntity(RpcArgs args)
		{
			if (!args.Info.SendingPlayer.IsHost)
			{
				throw new System.InvalidOperationException("NetworkInputController => Only the server can set the active entity.");
			}

			uint entityId = args.GetNext<uint>();

            MainThreadManager.Run(() =>
            {
                StartCoroutine(TryFindEntityWithId(entityId));
            });
		}

		public override void SyncInputs(RpcArgs args)
		{
			if (!networkObject.IsServer)
			{
				throw new System.InvalidOperationException("NetworkInputController => Only the server can receive input!");
			}

			byte[] inputs = args.GetNext<byte[]>();
			List<InputFrame> inputToAdd = ByteHelpers.ByteArrayToObject(inputs) as List<InputFrame>;
			inputToPlay.AddRange(inputToAdd);
		}

		public override void SyncNetEntityStatusHistory(RpcArgs args)
		{
			byte[] history = args.GetNext<byte[]>();
			uint owner = args.GetNext<uint>();

			if (networkObject.ownerId != owner)
			{
				return;
			}

			List<NetEntityStatus> entStatuses = ByteHelpers.ByteArrayToObject(history) as List<NetEntityStatus>;
			authStatusHistory.AddRange(entStatuses);
		}


        // Coroutine to attempt to get the entity with an entity id. if it fails, it'll restart itself 5 times.
        private IEnumerator TryFindEntityWithId(uint entityId)
        {
            Abstractions.NetworkControlledEntityBehavior result = FindObjectsOfType<Abstractions.NetworkControlledEntityBehavior>().FirstOrDefault(x => x.entityId == entityId);
			while (result == null)
			{
                Debug.Log(this.name + " => Could not find entity with id [" + entityId + "]. Retrying in one second...");
                yield return new WaitForSecondsRealtime(1f);
                result = FindObjectsOfType<Abstractions.NetworkControlledEntityBehavior>().FirstOrDefault(x => x.entityId == entityId);
                entityFetchAttemptsLeft--;

                if (entityFetchAttemptsLeft <= 0 && result == null)
                {
                    throw new System.NullReferenceException(this.name + " => Could not find entity with id [" + entityId + "] after " + entityFetchRetryAttempts + " retries, cancelling.");
                }
            }
            entityFetchAttemptsLeft = entityFetchRetryAttempts;
            Debug.Log(this.name + " => Set active controlled entity to Entity [" + entityId + "]");
			activeControlledEntity = result;
        }
	}
}