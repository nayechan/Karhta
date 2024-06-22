using System;
using Unity.VisualScripting;
using UnityEngine;

namespace InGame
{
    public class Boat : MonoBehaviour
    {
        public float speed = 10f;
        public float rotationSpeed = 100f;
        public float deceleration = 2f;
        public Transform boardTransform;
    
        private Rigidbody rb;
        private float moveInput, turnInput;
        private Player.Player player;
        [SerializeField] private bool isControlled = false;
        

        // Start is called before the first frame update
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        private void Update()
        {
            // Get input for forward/backward movement and rotation
            if (isControlled)
            {
                moveInput = Input.GetAxis("Vertical");
                turnInput = Input.GetAxis("Horizontal");
            }
            else
            {
                moveInput = 0;
                turnInput = 0;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) && player != null)
            {
                if (!isControlled)
                    Board(player.transform);

                else
                    Unboard(player.transform);
            }

        }

        private void FixedUpdate()
        {
            // Calculate the forward force and apply it to the Rigidbody
            var forwardForce = transform.forward * (moveInput * speed);

            // Apply acceleration and deceleration
            if (moveInput != 0)
            {
                rb.AddForce(forwardForce, ForceMode.Acceleration);
            }
            else
            {
                // Apply deceleration when there's no input
                rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, deceleration * Time.deltaTime);
            }

            // Calculate the rotation force and apply it to the Rigidbody
            var rotation = turnInput * rotationSpeed * Time.deltaTime;
            var turnRotation = Quaternion.Euler(0f, rotation, 0f);
            rb.MoveRotation(rb.rotation * turnRotation);
        }

        private void OnTriggerEnter(Collider other)
        {
            var collidedTransform = other.transform;
            if (collidedTransform.CompareTag("Player"))
            {
                 player = collidedTransform.GetComponent<Player.Player>();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var collidedTransform = other.transform;
            if (collidedTransform.CompareTag("Player"))
            {
                player = null;
            }
        }

        public void Board(Transform targetTransform)
        {
            targetTransform.GetComponent<CharacterController>().enabled = false;
            targetTransform.SetParent(boardTransform);
            targetTransform.localPosition = new Vector3(0, 0, 0);
            targetTransform.localRotation = Quaternion.identity;
            targetTransform.GetComponent<Animator>().SetBool("Boating", true);
            
            isControlled = true;
        }

        public void Unboard(Transform targetTransform)
        {
            targetTransform.GetComponent<CharacterController>().enabled = true;
            targetTransform.GetComponent<Animator>().SetBool("Boating", false);
            targetTransform.SetParent(null, true);
            targetTransform.localRotation = Quaternion.identity;
            isControlled = false;
        }
    }
}