using System;
using UnityEngine;

namespace Sburb.Constructs
{
    [System.Serializable]
    public class InputFrame
    {
        public float movementX;
        public float movementY;
        public uint frame;
        public bool isSprinting;
        public bool isJumping;
        public SerializableQuaternion rotation;

        public static InputFrame GetInput(uint _frame, Quaternion _rotation)
        {
            return new InputFrame
            {
                movementX = Input.GetAxis("Horizontal"),
                movementY = Input.GetAxis("Vertical"),
                frame = _frame,
                isSprinting = Input.GetButton("Sprint"),
                isJumping = Input.GetButton("Jump"),
                rotation = _rotation
            };
        }
    }
}