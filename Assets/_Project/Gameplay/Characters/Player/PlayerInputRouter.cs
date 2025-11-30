using ProjectRoguelike.Gameplay.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectRoguelike.Gameplay.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public sealed class PlayerInputRouter : MonoBehaviour
    {
        [SerializeField] private FpsMovementController movement;
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private bool lockCursorOnStart = true;

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _crouchAction;
        private InputAction _fireAction;
        private InputAction _altFireAction;
        private InputAction _reloadAction;
        private InputAction _abilityAction;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            if (_playerInput == null)
            {
                _playerInput = GetComponent<PlayerInput>();
            }

            if (_playerInput == null || _playerInput.actions == null)
            {
                Debug.LogWarning("[PlayerInputRouter] PlayerInput component or actions not found. Make sure PlayerInput is attached and Actions asset is assigned.");
                return;
            }

            CacheActions();
            Subscribe();

            if (lockCursorOnStart)
            {
                LockCursor();
            }
        }

        private void Update()
        {
            // Keep cursor locked during gameplay
            if (lockCursorOnStart && Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void CacheActions()
        {
            var map = _playerInput.actions.FindActionMap("Gameplay", throwIfNotFound: true);
            _moveAction = map.FindAction("Move", true);
            _lookAction = map.FindAction("Look", true);
            _jumpAction = map.FindAction("Jump", true);
            _sprintAction = map.FindAction("Sprint", true);
            _crouchAction = map.FindAction("Crouch", true);
            _fireAction = map.FindAction("Fire", true);
            _altFireAction = map.FindAction("AltFire", true);
            _reloadAction = map.FindAction("Reload", true);
            _abilityAction = map.FindAction("Ability", true);
        }

        private void Subscribe()
        {
            _moveAction.performed += HandleMove;
            _moveAction.canceled += HandleMoveCancel;

            _lookAction.performed += HandleLook;
            _lookAction.canceled += HandleLookCancel;

            _jumpAction.performed += HandleJump;

            _sprintAction.performed += HandleSprintStart;
            _sprintAction.canceled += HandleSprintEnd;

            _crouchAction.performed += HandleCrouchStart;
            _crouchAction.canceled += HandleCrouchEnd;

            _fireAction.performed += HandlePrimaryFireStart;
            _fireAction.canceled += HandlePrimaryFireEnd;

            _altFireAction.performed += HandleAltFireStart;
            _altFireAction.canceled += HandleAltFireEnd;

            _reloadAction.performed += HandleReload;
            _abilityAction.performed += HandleAbility;
        }

        private void Unsubscribe()
        {
            _moveAction.performed -= HandleMove;
            _moveAction.canceled -= HandleMoveCancel;
            _lookAction.performed -= HandleLook;
            _lookAction.canceled -= HandleLookCancel;
            _jumpAction.performed -= HandleJump;
            _sprintAction.performed -= HandleSprintStart;
            _sprintAction.canceled -= HandleSprintEnd;
            _crouchAction.performed -= HandleCrouchStart;
            _crouchAction.canceled -= HandleCrouchEnd;
            _fireAction.performed -= HandlePrimaryFireStart;
            _fireAction.canceled -= HandlePrimaryFireEnd;
            _altFireAction.performed -= HandleAltFireStart;
            _altFireAction.canceled -= HandleAltFireEnd;
            _reloadAction.performed -= HandleReload;
            _abilityAction.performed -= HandleAbility;
        }

        private void HandleMove(InputAction.CallbackContext context) => movement.SetMoveInput(context.ReadValue<Vector2>());
        private void HandleMoveCancel(InputAction.CallbackContext _) => movement.SetMoveInput(Vector2.zero);
        private void HandleLook(InputAction.CallbackContext context) => movement.SetLookInput(context.ReadValue<Vector2>());
        private void HandleLookCancel(InputAction.CallbackContext _) => movement.SetLookInput(Vector2.zero);
        private void HandleJump(InputAction.CallbackContext _) => movement.QueueJump(true);
        private void HandleSprintStart(InputAction.CallbackContext _) => movement.SetSprint(true);
        private void HandleSprintEnd(InputAction.CallbackContext _) => movement.SetSprint(false);
        private void HandleCrouchStart(InputAction.CallbackContext _) => movement.SetCrouch(true);
        private void HandleCrouchEnd(InputAction.CallbackContext _) => movement.SetCrouch(false);
        private void HandlePrimaryFireStart(InputAction.CallbackContext _) => weaponController?.StartPrimaryFire();
        private void HandlePrimaryFireEnd(InputAction.CallbackContext _) => weaponController?.StopPrimaryFire();
        private void HandleAltFireStart(InputAction.CallbackContext _) => weaponController?.StartAltFire();
        private void HandleAltFireEnd(InputAction.CallbackContext _) => weaponController?.StopAltFire();
        private void HandleReload(InputAction.CallbackContext _) => weaponController?.ManualReload();
        private void HandleAbility(InputAction.CallbackContext _) => weaponController?.TriggerAbility();
    }
}

