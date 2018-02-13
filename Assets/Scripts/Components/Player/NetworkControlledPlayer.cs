using Sburb.Abstractions;
using Sburb.Constructs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Sburb.Components
{
	// TODO: Fix this. Or test it I don't remember.
	public sealed class NetworkControlledPlayer : NetworkControlledEntityBehavior
	{
		
		[SerializeField]
		private float moveSpeed;
		[SerializeField]
		private float sprintSpeed;
		[SerializeField]
		private float jumpForce;
		[SerializeField]
		private float gravity;

		private CharacterController controller;
		private Vector3 moveVector;

		void Start()
		{
			controller = GetComponent<CharacterController>();
			moveVector = Vector3.zero;
		}

		void Update()
		{
			// Get server values if we don't own this instance and we aren't the server.
			if (!networkObject.IsServer && ownerId != networkObject.MyPlayerId)
			{
				GetServerValues();
			}
		}

		public override void PerformMovement(InputFrame frame, bool setServerRotation)
		{
			if (frame == null)
			{
				return;
			}

			if (setServerRotation)
			{
				transform.rotation = frame.rotation;
			}

			moveVector = controller.velocity * Time.deltaTime;

			if (controller.isGrounded)
			{
				moveVector = new Vector3(frame.movementX, 0, frame.movementY);
				moveVector = transform.TransformDirection(moveVector);
				if (frame.isSprinting)
				{
					moveVector *= sprintSpeed;
				}
				else
				{
					moveVector *= moveSpeed;
				}

				if (frame.isJumping)
				{
					moveVector.y += jumpForce;
				}
			}

			moveVector.y -= gravity * Time.deltaTime;
			controller.Move(moveVector);
		}

		public override NetEntityStatus GetNetEntityStatus(uint _frame)
		{
			NetEntityStatus status = new NetEntityStatus
			{
				position = transform.position,
				rotation = transform.rotation,
				frame = _frame
			};

			// TODO: Implement player stats result.
			return status;
		}

		public override void GetServerValues()
		{
			transform.position = networkObject.position;
			transform.rotation = networkObject.rotation;
		}

		public override void SetServerValues()
		{
			networkObject.position = transform.position;
			networkObject.rotation = transform.rotation;
		}

		public override void ReconciliateClient(ref List<NetEntityStatus> localHistory, ref List<NetEntityStatus> serverHistory)
		{
			foreach (NetEntityStatus history in serverHistory)
			{
				NetEntityStatus correspondingStatus = localHistory.FirstOrDefault(x => x.frame == history.frame);

                StringBuilder debug = new StringBuilder();
                
                debug.AppendLine("== Reconciliation == ");
                debug.AppendLine("History Frame: " + history.frame);
                debug.AppendLine("Upmost Local Frame: " + localHistory.Any());
                debug.AppendLine("Null? " + (correspondingStatus == null).ToString());

                Debug.Log(debug);

                if (correspondingStatus == null)
                {
                    NetEntityStatus reconciliationStatus = serverHistory.LastOrDefault();
                    transform.position = reconciliationStatus.position;
                    localHistory.RemoveAll(x => x.frame <= reconciliationStatus.frame);
                    serverHistory.RemoveAll(x => x.frame <= reconciliationStatus.frame);
                    return;
                }
				float distance = Vector3.Distance(correspondingStatus.position, history.position);

				if (distance > SBURB.Constants.MAX_RECONCILIATION_DISTANCE)
				{
                    NetEntityStatus reconciliationStatus = serverHistory.LastOrDefault();
                    transform.position = reconciliationStatus.position;
                    localHistory.RemoveAll(x => x.frame <= reconciliationStatus.frame);
                    serverHistory.RemoveAll(x => x.frame <= reconciliationStatus.frame);
                    return;
                }
				else
				{
					serverHistory.RemoveAll(x => x.frame <= history.frame);
					localHistory.RemoveAll(x => x.frame <= correspondingStatus.frame);
				}
			}
		}
	}
}
