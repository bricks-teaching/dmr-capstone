using UnityEngine;

namespace FiscalShock.Controls {
    public class MouseLook : MonoBehaviour
    {
        public bool lockCursorToGame = true;
        public float mouseSensitivity = 100f;
        public float clampMinimum = -90f;
        public float clampMaximum = 90f;

        public Transform body;

        private float xRotation = 0f;

        // Update is called once per frame
        public void Update() {
            if (lockCursorToGame) {
                Cursor.lockState = CursorLockMode.Locked;
            }
            // Moves the camera with the mouse, uses Time.deltaTime for FPS correction (Independent of current Frame rate)
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;

            //Cannot look further than 90 degrees up
            xRotation = Mathf.Clamp(xRotation, clampMinimum, clampMaximum);

            transform.localRotation = Quaternion.Euler(xRotation,0f,0f);
            body.Rotate(Vector3.up * mouseX);
        }
    }
}
