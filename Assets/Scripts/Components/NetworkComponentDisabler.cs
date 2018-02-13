using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using UnityEngine;

// This is used to tell the NCD whether to set the "enabled" Behaviour bool to false, or call it on the GameObject.
[System.Serializable]
public struct ComponentModel
{
    public Behaviour behaviour;

    // If false, Behaviour.enabled will be set to false. Else Behaviour.gameObject.SetActive(false) will be called. Same for enabling.
    public bool callOnGameObject;
}

public class NetworkComponentDisabler : NetworkComponentDisablerBehavior
{
	[SerializeField]
	private uint ownerId;
	[SerializeField]
	private ComponentModel[] componentsToDisable;
	private bool isReady = false;

	void Update()
	{
        if (!isReady)
        {
            return;
        }

		if (ownerId == networkObject.MyPlayerId)
		{
			foreach (ComponentModel component in componentsToDisable)
			{
				if (component.callOnGameObject)
                {
                    component.behaviour.gameObject.SetActive(true);
                }
                else
                {
                    component.behaviour.enabled = true;
                }
			}
		}
		else
		{
			foreach (ComponentModel component in componentsToDisable)
			{
                if (component.callOnGameObject)
                {
                    component.behaviour.gameObject.SetActive(false);
                }
                else
                {
                    component.behaviour.enabled = false;
                }
            }
		}
	}

	public override void SetOwnerId(RpcArgs args)
	{
		if (!args.Info.SendingPlayer.IsHost)
		{
			throw new System.InvalidOperationException("NetworkComponentDisabler => Only the server can set owner id");
		}

		uint networkId = args.GetNext<uint>();
		ownerId = networkId;
        isReady = true;
        Debug.Log("NetworkComponentDisabler [" + networkId + "] => Ready!");
	}
}
