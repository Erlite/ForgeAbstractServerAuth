using BeardedManStudios.Forge.Networking;
using ForgeNetworkControlledEntityBehavior = BeardedManStudios.Forge.Networking.Generated.NetworkControlledEntityBehavior;
using Sburb.Constructs;
using System.Collections.Generic;
using UnityEngine;

namespace Sburb.Abstractions
{
	public abstract class NetworkControlledEntityBehavior : ForgeNetworkControlledEntityBehavior
	{

		/// <summary>
		/// Network Id of the object that has control over this entity.
		/// </summary>
		public uint ownerId;

		/// <summary>
		/// The unique Id of this entity. Synchronized over network via RPC, set by server.
		/// </summary>
		public uint entityId;

		/// <summary>
		/// Perform a movement based on the input received by the owner.
		/// </summary>
		abstract public void PerformMovement(InputFrame frame, bool setServerRotation);

		/// <summary>
		/// Reconciliate the client's position based on the server's entity history and the clients.
		/// Should check for inconsistencies using the MAX_RECONCILIATION_DISTANCE.
		/// </summary>
		abstract public void ReconciliateClient(ref List<NetEntityStatus> localHistory, ref List<NetEntityStatus> serverHistory);

		/// <summary>
		/// Get the status of an entity after performing movement. Used for reconciliation checks.
		/// </summary>
		abstract public NetEntityStatus GetNetEntityStatus(uint _frame);

		/// <summary>
		/// Get values from the server if this gameObject is on a client that doesn't own it.
		/// Example: position and rotation.
		/// </summary>
		abstract public void GetServerValues();

		/// <summary>
		/// Set server values for clients to get if they don't own this gameObject.
		/// Example: position and rotation.
		/// </summary>
		abstract public void SetServerValues();

		public override void SetOwnerId(RpcArgs args)
		{
			if (!args.Info.SendingPlayer.IsHost)	
			{
				throw new System.InvalidOperationException(this.name + " => Only the server can send a SetOwnerId RPC.");
			}

			ownerId = args.GetNext<uint>();
		}

		public override void SetEntityId(RpcArgs args)
		{
			if (!args.Info.SendingPlayer.IsHost)
			{
				throw new System.InvalidOperationException(this.name + " => Only the server can set the entity's ID");
			}

			entityId = args.GetNext<uint>();
		}
	}
}