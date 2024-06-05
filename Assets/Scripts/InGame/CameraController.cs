using System;
using System.Linq;
using UnityEngine;

namespace InGame
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
        
        private Vector3 adjustedTargetPosition => target.position + offset;

        private void Start()
        {
            adjustedZoomDistance = currentZoomDistance;
        }

        private void Update()
        {

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

            foreach (Vector3 viewportPoint in viewportPoints)
            {
                var cameraPosition = mainCamera.ScreenToWorldPoint(viewportPoint);
                Ray ray = new Ray(adjustedTargetPosition, cameraPosition - adjustedTargetPosition);
                Debug.DrawRay(ray.origin, ray.direction * (minDistance + 1), Color.red);

                var layersToIgnore = new[] { "Water", "Player" };
                var layerMask = layersToIgnore.Aggregate(
                    0,
                    (current, layer) => current | (1 << LayerMask.NameToLayer(layer))
                );
                layerMask = ~layerMask;

                var hits = Physics.RaycastAll(ray, minDistance - 1, layerMask);

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

        public void RotateCamera(float mouseSensitivity)
        {
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            var currentRotation = transform.rotation.eulerAngles;

            currentRotation.x = Mathf.Clamp(currentRotation.x - mouseY, 15, 80);
            currentRotation.y = target.transform.rotation.eulerAngles.y;
            currentRotation.z = 0;
    
            // Get the target's rotation in the world space
            Quaternion targetRotation = Quaternion.Euler(currentRotation);

            // Apply the target's rotation to the camera
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed);
        }

    }
}
