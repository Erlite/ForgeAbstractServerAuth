using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

namespace BeardedManStudios.Forge.Networking.Generated
{
	[GeneratedInterpol("{\"inter\":[0]")]
	public partial class NetworkedInputControllerNetworkObject : NetworkObject
	{
		public const int IDENTITY = 7;

		private byte[] _dirtyFields = new byte[1];

		#pragma warning disable 0067
		public event FieldChangedEvent fieldAltered;
		#pragma warning restore 0067
		private uint _ownerId;
		public event FieldEvent<uint> ownerIdChanged;
		public Interpolated<uint> ownerIdInterpolation = new Interpolated<uint>() { LerpT = 0f, Enabled = false };
		public uint ownerId
		{
			get { return _ownerId; }
			set
			{
				// Don't do anything if the value is the same
				if (_ownerId == value)
					return;

				// Mark the field as dirty for the network to transmit
				_dirtyFields[0] |= 0x1;
				_ownerId = value;
				hasDirtyFields = true;
			}
		}

		public void SetownerIdDirty()
		{
			_dirtyFields[0] |= 0x1;
			hasDirtyFields = true;
		}

		private void RunChange_ownerId(ulong timestep)
		{
			if (ownerIdChanged != null) ownerIdChanged(_ownerId, timestep);
			if (fieldAltered != null) fieldAltered("ownerId", _ownerId, timestep);
		}

		protected override void OwnershipChanged()
		{
			base.OwnershipChanged();
			SnapInterpolations();
		}
		
		public void SnapInterpolations()
		{
			ownerIdInterpolation.current = ownerIdInterpolation.target;
		}

		public override int UniqueIdentity { get { return IDENTITY; } }

		protected override BMSByte WritePayload(BMSByte data)
		{
			UnityObjectMapper.Instance.MapBytes(data, _ownerId);

			return data;
		}

		protected override void ReadPayload(BMSByte payload, ulong timestep)
		{
			_ownerId = UnityObjectMapper.Instance.Map<uint>(payload);
			ownerIdInterpolation.current = _ownerId;
			ownerIdInterpolation.target = _ownerId;
			RunChange_ownerId(timestep);
		}

		protected override BMSByte SerializeDirtyFields()
		{
			dirtyFieldsData.Clear();
			dirtyFieldsData.Append(_dirtyFields);

			if ((0x1 & _dirtyFields[0]) != 0)
				UnityObjectMapper.Instance.MapBytes(dirtyFieldsData, _ownerId);

			// Reset all the dirty fields
			for (int i = 0; i < _dirtyFields.Length; i++)
				_dirtyFields[i] = 0;

			return dirtyFieldsData;
		}

		protected override void ReadDirtyFields(BMSByte data, ulong timestep)
		{
			if (readDirtyFlags == null)
				Initialize();

			Buffer.BlockCopy(data.byteArr, data.StartIndex(), readDirtyFlags, 0, readDirtyFlags.Length);
			data.MoveStartIndex(readDirtyFlags.Length);

			if ((0x1 & readDirtyFlags[0]) != 0)
			{
				if (ownerIdInterpolation.Enabled)
				{
					ownerIdInterpolation.target = UnityObjectMapper.Instance.Map<uint>(data);
					ownerIdInterpolation.Timestep = timestep;
				}
				else
				{
					_ownerId = UnityObjectMapper.Instance.Map<uint>(data);
					RunChange_ownerId(timestep);
				}
			}
		}

		public override void InterpolateUpdate()
		{
			if (IsOwner)
				return;

			if (ownerIdInterpolation.Enabled && !ownerIdInterpolation.current.UnityNear(ownerIdInterpolation.target, 0.0015f))
			{
				_ownerId = (uint)ownerIdInterpolation.Interpolate();
				//RunChange_ownerId(ownerIdInterpolation.Timestep);
			}
		}

		private void Initialize()
		{
			if (readDirtyFlags == null)
				readDirtyFlags = new byte[1];

		}

		public NetworkedInputControllerNetworkObject() : base() { Initialize(); }
		public NetworkedInputControllerNetworkObject(NetWorker networker, INetworkBehavior networkBehavior = null, int createCode = 0, byte[] metadata = null) : base(networker, networkBehavior, createCode, metadata) { Initialize(); }
		public NetworkedInputControllerNetworkObject(NetWorker networker, uint serverId, FrameStream frame) : base(networker, serverId, frame) { Initialize(); }

		// DO NOT TOUCH, THIS GETS GENERATED PLEASE EXTEND THIS CLASS IF YOU WISH TO HAVE CUSTOM CODE ADDITIONS
	}
}
