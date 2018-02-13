using UnityEngine;

namespace Sburb.Constructs
{
    [System.Serializable]
    public class NetEntityStatus
    {
        public SerializableVector3 position;
        public SerializableQuaternion rotation;
        public uint frame;
    }
}