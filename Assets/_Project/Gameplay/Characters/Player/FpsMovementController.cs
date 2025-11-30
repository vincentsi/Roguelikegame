using UnityEngine;

namespace ProjectRoguelike.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class FpsMovementController : MonoBehaviour
    {
        private enum MovementState
        {
            Idle,
            Move,
            Sprint,
            Slide,
            Air
        }

        [Header("Dependencies")]
        [SerializeField] private PlayerCameraController cameraController;
        [SerializeField] private PlayerStatsComponent stats;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float sprintSpeed = 7.5f;
        [SerializeField] private float crouchSpeed = 3f;
        [SerializeField] private float acceleration = 16f;
        [SerializeField] private float airControlPercent = 0.35f;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float coyoteTime = 0.2f;

        [Header("Slide")]
        [SerializeField] private float slideDuration = 0.9f;
        [SerializeField] private float slideSpeed = 9f;
        [SerializeField] private AnimationCurve slideSpeedCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Camera")]
        [SerializeField] private float lookSensitivity = 50f;
        [SerializeField] private float headBobAmplitude = 0.05f;
        [SerializeField] private float headBobFrequency = 12f;

        private CharacterController _controller;
        private Vector2 _moveInput;
        private Vector2 _lookInput;
#pragma warning disable CS0414 // Field is assigned but never used (reserved for animations/debug)
        private MovementState _state;
#pragma warning restore CS0414
        private Vector3 _velocity;
        private float _verticalVelocity;
        private float _lastGroundedTime;
        private bool _jumpQueued;
        private bool _sprintHeld;
        private bool _crouchHeld;
        private bool _slideActive;
        private float _slideTimer;
        private Vector3 _slideDirection;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _state = MovementState.Idle;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            HandleLook(deltaTime);
            HandleMovement(deltaTime);
        }

        public void SetMoveInput(Vector2 value)
        {
            _moveInput = Vector2.ClampMagnitude(value, 1f);
        }

        public void SetLookInput(Vector2 value)
        {
            _lookInput = value * lookSensitivity;
        }

        public void SetSprint(bool pressed)
        {
            _sprintHeld = pressed;
        }

        public void SetCrouch(bool pressed)
        {
            if (pressed && !_crouchHeld && CanTriggerSlide())
            {
                BeginSlide();
            }

            _crouchHeld = pressed;
        }

        public void QueueJump(bool pressed)
        {
            if (pressed)
            {
                _jumpQueued = true;
            }
        }

        private void HandleLook(float deltaTime)
        {
            if (cameraController == null)
            {
                return;
            }

            cameraController.ApplyLook(_lookInput, deltaTime);
        }

        private void HandleMovement(float deltaTime)
        {
            bool grounded = _controller.isGrounded;
            if (grounded)
            {
                _lastGroundedTime = Time.time;
                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = -2f;
                }
            }
            else
            {
                _verticalVelocity += gravity * deltaTime;
            }

            if (_jumpQueued && (grounded || Time.time - _lastGroundedTime <= coyoteTime))
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _jumpQueued = false;
                grounded = false;
            }
            else if (_jumpQueued && !grounded)
            {
                // Keep the request until coyote expires to feel responsive.
                if (Time.time - _lastGroundedTime > coyoteTime)
                {
                    _jumpQueued = false;
                }
            }

            Vector3 inputDir = new Vector3(_moveInput.x, 0f, _moveInput.y);
            inputDir = transform.TransformDirection(inputDir);
            float targetSpeed = DetermineTargetSpeed(grounded);

            if (_slideActive)
            {
                UpdateSlide(deltaTime);
                inputDir = _slideDirection;
                targetSpeed = slideSpeed * slideSpeedCurve.Evaluate(1f - (_slideTimer / slideDuration));
            }

            float accelerationFactor = grounded ? acceleration : acceleration * airControlPercent;
            Vector3 desiredVelocity = inputDir * targetSpeed;
            _velocity = Vector3.MoveTowards(_velocity, desiredVelocity, accelerationFactor * deltaTime);
            Vector3 frameVelocity = _velocity;
            frameVelocity.y = _verticalVelocity;
            _controller.Move(frameVelocity * deltaTime);

            UpdateStateMachine(grounded, targetSpeed);
            ApplyHeadBob(deltaTime);
        }

        private float DetermineTargetSpeed(bool grounded)
        {
            if (_slideActive)
            {
                return slideSpeed;
            }

            if (_moveInput.sqrMagnitude <= 0.01f)
            {
                return 0f;
            }

            if (_crouchHeld)
            {
                return crouchSpeed;
            }

            if (_sprintHeld && grounded && stats != null && stats.TrySpendSprintStamina(Time.deltaTime))
            {
                return sprintSpeed;
            }

            return walkSpeed;
        }

        private bool CanTriggerSlide()
        {
            return !_slideActive && _controller.isGrounded && _sprintHeld && stats != null && stats.TrySpendSlideCost();
        }

        private void BeginSlide()
        {
            _slideActive = true;
            _slideTimer = slideDuration;
            _slideDirection = (_velocity.sqrMagnitude > 0.01f ? _velocity.normalized : transform.forward);
        }

        private void UpdateSlide(float deltaTime)
        {
            _slideTimer -= deltaTime;
            if (_slideTimer <= 0f || _controller.isGrounded == false)
            {
                _slideActive = false;
            }
        }

        private void UpdateStateMachine(bool grounded, float targetSpeed)
        {
            if (!grounded)
            {
                _state = MovementState.Air;
            }
            else if (_slideActive)
            {
                _state = MovementState.Slide;
            }
            else if (targetSpeed <= 0.1f)
            {
                _state = MovementState.Idle;
            }
            else if (Mathf.Approximately(targetSpeed, sprintSpeed))
            {
                _state = MovementState.Sprint;
            }
            else
            {
                _state = MovementState.Move;
            }
        }

        private void ApplyHeadBob(float deltaTime)
        {
            if (cameraController == null)
            {
                return;
            }

            float speedPercent = Mathf.Clamp01(_velocity.magnitude / sprintSpeed);
            float bobAmount = headBobAmplitude * speedPercent;
            float bobFrequency = headBobFrequency * speedPercent;
            cameraController.ApplyHeadBob(bobAmount, bobFrequency, deltaTime);
        }
    }
}

