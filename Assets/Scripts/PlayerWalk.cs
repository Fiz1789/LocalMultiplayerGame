using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace RowdyRobots
{
    public class PlayerWalk : MonoBehaviour
    {
        public InputActionAsset InputActions;
        // private InputAction m_moveAction;
        // private InputAction m_lookAction;
        // private InputAction m_jumpAction;
        // private InputAction m_crouchAction;
        // private InputAction m_sprintAction;
        public enum PlayerState { Idle, Walking, Crouching, CrouchWalking, Sprinting }
        private PlayerState currentState;
        private Vector2 m_moveAmt;
        private Vector2 m_lookAmt;
        private bool m_isCrouching;
        private bool m_isSprinting;

        private Animator m_animator;
        private Rigidbody m_rigidbody;
        private PlayerInput m_playerInput;

        [Header("Movement Settings")]
        public float dampSpeed = 2f;
        public float walkSpeed = 5f;

        public float sprintSpeed = 10f;

        // public float maxWalkSpeed = 5f;
        public float rotateSpeed = 5f;

        [Header("Crouch Settings")]
        public float crouchWalkSpeed = 2.5f;

        private float maxCrouchTime = 10f;

        private float currentCrouchTime = 0f; // Start at 0 so player starts standing

        private bool wasCrouching = false;


        [Header("Noise Settings")]
        [SerializeField] private float idleNoise = 0.1f;        // Standing idle
        [SerializeField] private float crouchIdleNoise = 0f;    // Crouch idle (silent)
        [SerializeField] private float walkNoise = 1f;          // Walking
        [SerializeField] private float crouchWalkNoise = 0.3f;  // Crouch walking
        [SerializeField] private float sprintNoise = 2f;        // Sprinting
        [SerializeField] private float standUpNoise = 0.5f;     // One-time: crouch → stand
        [Header("Noise Sphere")]
        [SerializeField] public float noiseRadiusMultiplier = 4f; // Noise value × this = sphere radius
        public bool SprintEnabled = true;
        public float currentNoise = 0f;
        public float currentNoiseRadius = 0f; // Calculated sphere radius

        public TrapSpawner trapSpawner; // Assign in inspector


        private void Awake()
        {
            // m_moveAction = InputSystem.actions.FindAction("Player/Move");
            // m_lookAction = InputSystem.actions.FindAction("Player/Look");
            // m_jumpAction = InputSystem.actions.FindAction("Player/Jump");

            // m_crouchAction = InputSystem.actions.FindAction("Player/Crouch");
            // m_sprintAction = InputSystem.actions.FindAction("Player/Sprint");

            m_animator = GetComponentInChildren<Animator>();
            m_rigidbody = GetComponent<Rigidbody>();
            m_playerInput = GetComponent<PlayerInput>();

        }

        private void OnEnable()
        {
            m_playerInput?.ActivateInput();
        }

        private void OnDisable()
        {
            m_playerInput?.DeactivateInput();
        }
        private void Update()
        {

            // m_moveAmt = m_moveAction.ReadValue<Vector2>();
            // m_lookAmt = m_lookAction.ReadValue<Vector2>();

            // Crouch logic (hold East Button)
            // m_isCrouching = m_crouchAction != null && m_crouchAction.ReadValue<float>() > 0.5f;
            // m_isSprinting = m_sprintAction != null && m_sprintAction.ReadValue<float>() > 0.5f;

            // State logic
            if (m_isCrouching)
            {
                if (!wasCrouching)
                {
                    // Only reset when starting a new crouch
                    currentCrouchTime = 0f;
                }
                currentState = (m_moveAmt.sqrMagnitude > 0.01f) ? PlayerState.CrouchWalking : PlayerState.Crouching;
                if (currentCrouchTime < maxCrouchTime)
                    currentCrouchTime += Time.deltaTime;
                else
                    currentCrouchTime = maxCrouchTime;
            }
            else if (m_isSprinting && m_moveAmt.sqrMagnitude > 0.01f && SprintEnabled)
            {
                currentState = PlayerState.Sprinting;
            }
            else
            {
                if (currentState == PlayerState.Crouching || currentState == PlayerState.CrouchWalking)
                {
                    // Smoothly transition out of crouch
                    if (currentCrouchTime > 0f)
                        currentCrouchTime -= Time.deltaTime;
                    else
                        currentCrouchTime = 0f;
                }
                currentState = (m_moveAmt.sqrMagnitude > 0.01f) ? PlayerState.Walking : PlayerState.Idle;
            }
            if (m_animator != null)
            {
                switch (currentState)
                {
                    case PlayerState.Idle:
                        m_animator.SetFloat("Speed", Mathf.Lerp(m_animator.GetFloat("Speed"), 0f, dampSpeed * Time.deltaTime));
                        m_animator.SetFloat("Crouch", Mathf.Lerp(m_animator.GetFloat("Crouch"), 0f, currentCrouchTime / maxCrouchTime));
                        break;
                    case PlayerState.Walking:
                        m_animator.SetFloat("Speed", Mathf.Lerp(m_animator.GetFloat("Speed"), 0.5f, dampSpeed * Time.deltaTime));
                        m_animator.SetFloat("Crouch", Mathf.Lerp(m_animator.GetFloat("Crouch"), 0f, currentCrouchTime / maxCrouchTime));
                        break;
                    case PlayerState.Crouching:
                        m_animator.SetFloat("Speed", Mathf.Lerp(m_animator.GetFloat("Speed"), 0f, dampSpeed * Time.deltaTime));
                        m_animator.SetFloat("Crouch", Mathf.Lerp(m_animator.GetFloat("Crouch"), 0.5f, currentCrouchTime / maxCrouchTime));
                        break;
                    case PlayerState.CrouchWalking:
                        m_animator.SetFloat("Speed", Mathf.Lerp(m_animator.GetFloat("Speed"), 0.5f, dampSpeed * Time.deltaTime));
                        m_animator.SetFloat("Crouch", Mathf.Lerp(m_animator.GetFloat("Crouch"), 0.5f, currentCrouchTime / maxCrouchTime));
                        break;
                    case PlayerState.Sprinting:
                        m_animator.SetFloat("Speed", Mathf.Lerp(m_animator.GetFloat("Speed"), 1f, dampSpeed * Time.deltaTime));
                        m_animator.SetFloat("Crouch", Mathf.Lerp(m_animator.GetFloat("Crouch"), 0f, currentCrouchTime / maxCrouchTime));
                        break;
                }
            }
            wasCrouching = m_isCrouching;

            UpdateNoise();
            // if (m_jumpAction.WasPressedThisFrame())
            // {
            //     Jump();
            // }
        }

        private void FixedUpdate()
        {
            Walking();
            Rotating();
        }

        public void Walking()
        {
            Vector2 input = m_moveAmt;
            if (input.sqrMagnitude > 0.01f)
            {
                input = input.normalized;
            }
            float effectiveSpeed = walkSpeed;
            if (currentState == PlayerState.CrouchWalking || currentState == PlayerState.Crouching)
                effectiveSpeed = crouchWalkSpeed;
            else if (currentState == PlayerState.Sprinting)
                effectiveSpeed = sprintSpeed;
            m_rigidbody.MovePosition(m_rigidbody.position + new Vector3(input.x, 0, input.y) * effectiveSpeed * Time.fixedDeltaTime);
        }
        public void OnMove(InputValue value)
        {

            m_moveAmt = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            m_lookAmt = value.Get<Vector2>();
        }
        public void OnCrouch(InputValue value)
        {
            // Debug.Log("Crouch value: " + value.Get<float>());
            //m_isCrouching = value.Get<float>() > 0.5f;
            m_isCrouching = value.isPressed;

        }
        public void OnSprint(InputValue value)
        {

            m_isSprinting = value.isPressed;

        }
        public void OnTrap()
        {

            if (trapSpawner != null && (trapSpawner.IsOnCooldown() || trapSpawner.HasReachedLimit()))
            {
                // On cooldown, do not play animation or spawn trap
                return;
            }
            if (trapSpawner != null)
            {
                trapSpawner.SpawnTrap(transform.position);
                m_animator.SetTrigger("PlaceTrap");
            }
            else
            {
                Debug.Log("trapSpawner is null");
            }

        }


        public void Rotating()
        {
            if (m_moveAmt.y != 0)
            {
                // float rotationAmount = m_lookAmt.x * RotateSpeed * Time.fixedDeltaTime * 2;
                // Quaternion deltaRotation = Quaternion.Euler(0,rotationAmount*2,0);
                // m_rigidbody.MoveRotation(m_rigidbody.rotation * deltaRotation);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(m_moveAmt.x, 0, m_moveAmt.y)), rotateSpeed * Time.fixedDeltaTime);
            }

        }
        private void UpdateNoise()
        {
            // Calculate base noise from current state
            float baseNoise = 0f;

            switch (currentState)
            {
                case PlayerState.Idle:
                    baseNoise = idleNoise;
                    break;
                case PlayerState.Crouching:
                    baseNoise = crouchIdleNoise;
                    break;
                case PlayerState.Walking:
                    baseNoise = walkNoise;
                    break;
                case PlayerState.CrouchWalking:
                    baseNoise = crouchWalkNoise;
                    break;
                case PlayerState.Sprinting:
                    baseNoise = sprintNoise;
                    break;
            }

            // Add one-time noise for standing up
            if (wasCrouching && !m_isCrouching && currentCrouchTime <= 0f)
            {
                baseNoise = Mathf.Max(baseNoise, standUpNoise);
                Debug.Log($"[Noise] Standing up! +{standUpNoise}");
            }

            // Only apply noise if moving (except idle)
            if (m_moveAmt.sqrMagnitude > 0.01f || currentState == PlayerState.Idle || currentState == PlayerState.Crouching)
            {
                currentNoise = baseNoise;
            }
            else
            {
                currentNoise = (currentState == PlayerState.Idle) ? idleNoise : 0f;
            }

            // Calculate noise sphere radius
            currentNoiseRadius = currentNoise * noiseRadiusMultiplier;
        }
        public void SetNoiseAndRadius(float value)
        {
            currentNoise = value;
            currentNoiseRadius = value * noiseRadiusMultiplier;
        }
        // DEBUG: Visualize noise sphere in Scene view
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            DrawNoiseSphere();
        }

        private void DrawNoiseSphere()
        {
            Color sphereColor = currentNoise > 0.5f ? Color.red : (currentNoise > 0.2f ? Color.yellow : Color.green);
            Gizmos.color = new Color(sphereColor.r, sphereColor.g, sphereColor.b, 0.3f);
            Gizmos.DrawSphere(transform.position, currentNoiseRadius);

            Gizmos.color = sphereColor;
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.DrawSphere(transform.position, 0.1f);

            Handles.Label(transform.position + Vector3.up * (currentNoiseRadius + 0.5f),
                $"Noise: {currentNoise:F1}\nRadius: {currentNoiseRadius:F1}m");
        }
#endif
        // public void Jump()
        // {
        //     m_rigidbody.AddForceAtPosition(new Vector3(0,5f,0),Vector3.up,ForceMode.Impulse);
        //     //m_animator.SetTrigger("Jump");
        // }

        public float CurrentNoise => currentNoise;
        public Vector3 NoisePosition => transform.position;

        public Vector2 LastMoveInput => m_moveAmt;
    }
}
