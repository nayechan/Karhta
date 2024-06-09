using UnityEngine;

namespace InGame.Player
{
    public class CameraController : MonoBehaviour
    {
        public Transform target; // The target to follow (usually the player character)
        public float smoothSpeed = 0.125f; // The speed at which the camera moves towards the target position
        public Vector3 offset; // The offset from the target position
        
        public float currentZoomDistance = 4f; // The current zoom distance
        public float minZoomDistance = 2f; // Minimum distance the camera can zoom in
        public float maxZoomDistance = 10f; // Maximum distance the camera can zoom out
        
        [SerializeField] private float adjustedZoomDistance;
        
        private float zoomVelocity; // SmoothDamp velocity for zooming
        private Vector3 currentVelocity; // SmoothDamp velocity for camera position

        [SerializeField] private float currentMouseOffset = 10;
        [SerializeField] private LayerMask layersToIgnore;

        [SerializeField] private bool disableAutoZoom = false;
        
        private Vector3 adjustedTargetPosition => target.position + offset;

        private void Start()
        {
            adjustedZoomDistance = currentZoomDistance;
        }

        private void Update()
        {
            var targetEulerRotation = transform.rotation.eulerAngles;
            targetEulerRotation.x = currentMouseOffset;
            targetEulerRotation.y = target.transform.rotation.eulerAngles.y;

            transform.rotation = Quaternion.Euler(targetEulerRotation);

            // Zoom in/out based on mouse scroll wheel
            currentZoomDistance -= Input.mouseScrollDelta.y;
            currentZoomDistance = Mathf.Clamp(currentZoomDistance, minZoomDistance, maxZoomDistance);

            // Calculate the desired camera position
            var desiredPosition = adjustedTargetPosition - (transform.forward * adjustedZoomDistance);

            // Smoothly interpolate the camera position towards the desired position
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, 
                ref currentVelocity, smoothSpeed);

            transform.position = desiredPosition;
        }
        private void FixedUpdate()
        {
            var mainCamera = Camera.main;

            var screenWidth = mainCamera.pixelWidth;
            var screenHeight = mainCamera.pixelHeight;
            
            // Check multiple points in the viewport
            Vector3[] viewportPoints = {
                new Vector3(0, 0, 1),   // Bottom-left
                new Vector3(0, screenHeight, 1),   // Top-left
                new Vector3(screenWidth, 0, 1),   // Bottom-right
                new Vector3(screenWidth, screenHeight, 1),   // Top-right
                new Vector3(0.5f * screenWidth, 0.5f * screenHeight, 1)  // Center
            };

            float minDistance = currentZoomDistance;
            adjustedZoomDistance = currentZoomDistance;

            if (!disableAutoZoom)
            {
                foreach (Vector3 viewportPoint in viewportPoints)
                {
                    var cameraPosition = mainCamera.ScreenToWorldPoint(viewportPoint);
                    Ray ray = new Ray(adjustedTargetPosition, cameraPosition - adjustedTargetPosition);
                    Debug.DrawRay(ray.origin, ray.direction * (minDistance + 1), Color.red);

                    var hits = Physics.RaycastAll(ray, minDistance - 1, ~layersToIgnore);

                    foreach (var hit in hits)
                    {
                        if (hit.distance < minDistance)
                        {
                            Debug.Log(hit.transform.name);
                            minDistance = hit.distance;
                        }
                    }
                }

                if (minDistance < 1.5f) minDistance = 1.5f;

                // Smoothly adjust the zoom distance based on the closest hit distance
                if (minDistance < adjustedZoomDistance)
                {
                    //adjustedZoomDistance = Mathf.SmoothDamp(adjustedZoomDistance, minDistance, 
                    //    ref zoomVelocity, smoothSpeed);
                    adjustedZoomDistance = minDistance;
                }
            }
        }

        public void RotateCamera(float mouseSensitivity)
        {
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            currentMouseOffset = Mathf.Clamp(currentMouseOffset + mouseY, 10, 80);
        }

    }
}
