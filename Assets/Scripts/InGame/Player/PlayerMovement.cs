using UnityEngine;

namespace InGame.Player
{
    public partial class Player : MonoBehaviour
    {
        private partial void ApplyGravity()
        {
            if ((IsGrounded() && playerVelocity.y < 0) || 
             !isGravityPresent)
            {
                playerVelocity.y = 0f;
            }
            
            // Apply gravity
            if (isGravityPresent)
            {
                if (!isUnderWater)
                {
                    playerVelocity.y += gravityValue * Time.deltaTime; 
                }
                else
                {
                    playerVelocity.y += gravityValue / 2.0f * Time.deltaTime; 
                }
            }
        }
        
        private partial void TeleportToPosition(Vector3 newPosition)
        {
            characterController.enabled = false;
            transform.position = newPosition;
            characterController.enabled = true;
        }

        private partial void Move(float horizontalInput, float verticalInput)
        {
            var horizontalMovement = horizontalInput * moveSpeed;
            var verticalMovement = verticalInput * moveSpeed;

            if (horizontalMovement > 0)
                horizontalMovement *= speedMultiplier;

            if (verticalMovement > 0)
                verticalMovement *= speedMultiplier;

            Vector3 movement = horizontalMovement * transform.right;
            movement += verticalMovement * transform.forward;
            movement *= speedMultiplier;

            if (isUnderWater)
                movement *= 0.3f;

            // Move the character controller
            characterController.Move(movement * Time.deltaTime + playerVelocity * Time.deltaTime);
            animator.SetFloat(HorizontalSpeed, horizontalInput);
            animator.SetFloat(VerticalSpeed, verticalInput / 2.0f * speedMultiplier);
        }

        private partial void Jump()
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            animator.SetTrigger(JumpTrigger);
        }

        private partial void Float()
        {
            playerVelocity.y -= 20.0f * Time.deltaTime; 
        }
        
        private partial void RotatePlayer()
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up * mouseX, Space.Self);
            cameraController.RotateCamera(rotationSpeed);
        }

        public partial void SetIsGravityPresent(bool state)
        {
            isGravityPresent = state;
        }
    }
}