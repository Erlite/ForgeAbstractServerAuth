using UnityEngine;

namespace Sburb.Components
{
	public class PlayerCameraController : MonoBehaviour
	{
		public float sensitivity = 6f;
		public float smoothing = 1.6f;
		private Camera cam;
		private Vector2 smoothedVector;
		private Vector2 mouseLook;
		
		void Update()
		{
            // Set the camera, might be disabled on startup, so just keep getting until it's there.
            cam = cam ?? GetComponentInChildren<Camera>();

            // If the camera isn't ready yet, return.
            if (cam == null)
            {
                return;
            }

			float smoothedSensitivity = sensitivity * smoothing;

			Vector2 mouseDirection = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
			mouseDirection = Vector2.Scale(mouseDirection, new Vector2(smoothedSensitivity, smoothedSensitivity));

			smoothedVector.x = Mathf.Lerp(smoothedVector.x, mouseDirection.x, 1f / smoothing);
			smoothedVector.y = Mathf.Lerp(smoothedVector.y, mouseDirection.y, 1f / smoothing);
			mouseLook += smoothedVector;

			mouseLook.y = Mathf.Clamp(mouseLook.y, -90f, 90f);

			cam.transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
			transform.localRotation = Quaternion.AngleAxis(mouseLook.x, transform.up);
		}
	}
}