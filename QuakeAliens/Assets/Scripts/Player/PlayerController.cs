using UnityEngine;

namespace QuakeAliens.Player
{
    /// <summary>
    /// Quake-style player movement controller with strafe jumping, bunny hopping, and air control
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 7f;
        [SerializeField] private float runSpeed = 11f;
        [SerializeField] private float airAcceleration = 2f;
        [SerializeField] private float groundAcceleration = 10f;
        [SerializeField] private float friction = 6f;
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float jumpForce = 8.5f;
        [SerializeField] private float airControl = 0.3f;

        [Header("Mouse Look")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 89f;
        [SerializeField] private Transform cameraTransform;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private LayerMask groundMask;

        private CharacterController controller;
        private Vector3 velocity;
        private Vector3 moveDirection;
        private float verticalRotation = 0f;
        private bool isGrounded;
        private bool wishJump;

        // Quake-style movement variables
        private float currentSpeed;
        private Vector3 wishDir;
        private float wishSpeed;

        public float Health { get; private set; } = 100f;
        public float MaxHealth { get; private set; } = 100f;
        public bool IsDead => Health <= 0;

        public static PlayerController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            controller = GetComponent<CharacterController>();
            
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (IsDead) return;

            HandleMouseLook();
            CheckGround();
            HandleInput();
            
            if (isGrounded)
            {
                GroundMove();
            }
            else
            {
                AirMove();
            }

            // Apply gravity
            velocity.y -= gravity * Time.deltaTime;

            // Move the controller
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Horizontal rotation - rotate the player
            transform.Rotate(Vector3.up * mouseX);

            // Vertical rotation - rotate the camera
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
            
            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            }
        }

        private void CheckGround()
        {
            isGrounded = controller.isGrounded;
            
            // Additional raycast check for better ground detection
            if (Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + controller.height / 2f, groundMask))
            {
                isGrounded = true;
            }
        }

        private void HandleInput()
        {
            // Get WASD input
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            // Calculate wish direction in world space
            wishDir = transform.right * horizontal + transform.forward * vertical;
            wishDir.Normalize();

            // Determine target speed
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            wishSpeed = isRunning ? runSpeed : walkSpeed;

            // Queue jump - Quake style allows holding jump before landing
            if (Input.GetButton("Jump"))
            {
                wishJump = true;
            }
        }

        private void GroundMove()
        {
            // Apply friction
            ApplyFriction(1.0f);

            // Check for jump
            if (wishJump)
            {
                velocity.y = jumpForce;
                wishJump = false;
            }

            // Calculate the target velocity
            float currentSpeedInWishDir = Vector3.Dot(new Vector3(velocity.x, 0, velocity.z), wishDir);
            float addSpeed = wishSpeed - currentSpeedInWishDir;

            if (addSpeed <= 0)
                return;

            float accelSpeed = groundAcceleration * Time.deltaTime * wishSpeed;
            if (accelSpeed > addSpeed)
                accelSpeed = addSpeed;

            velocity.x += accelSpeed * wishDir.x;
            velocity.z += accelSpeed * wishDir.z;
        }

        private void AirMove()
        {
            // Quake-style air strafing
            float currentSpeedInWishDir = Vector3.Dot(new Vector3(velocity.x, 0, velocity.z), wishDir);
            float addSpeed = wishSpeed * airControl - currentSpeedInWishDir;

            if (addSpeed <= 0)
                return;

            float accelSpeed = airAcceleration * Time.deltaTime * wishSpeed;
            if (accelSpeed > addSpeed)
                accelSpeed = addSpeed;

            velocity.x += accelSpeed * wishDir.x;
            velocity.z += accelSpeed * wishDir.z;

            // Reset jump wish in air
            if (!Input.GetButton("Jump"))
            {
                wishJump = false;
            }
        }

        private void ApplyFriction(float frictionMultiplier)
        {
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            float speed = horizontalVelocity.magnitude;

            if (speed < 0.1f)
            {
                velocity.x = 0;
                velocity.z = 0;
                return;
            }

            float drop = speed * friction * frictionMultiplier * Time.deltaTime;
            float newSpeed = Mathf.Max(speed - drop, 0);
            newSpeed /= speed;

            velocity.x *= newSpeed;
            velocity.z *= newSpeed;
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            Health -= damage;
            Health = Mathf.Max(Health, 0);

            // Screen shake effect could be added here
            if (Health <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            Health = Mathf.Min(Health + amount, MaxHealth);
        }

        private void Die()
        {
            Debug.Log("Player died!");
            // Trigger game over
            GameManager.Instance?.GameOver();
        }

        public void Respawn(Vector3 position)
        {
            Health = MaxHealth;
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
            velocity = Vector3.zero;
        }
    }
}

