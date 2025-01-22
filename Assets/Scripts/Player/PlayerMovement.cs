using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float rollSpeed = 8f;
    [SerializeField] private float rollDuration = 0.6f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedGravity = -2f;
    
    [Header("Combat")]
    [SerializeField] private float lockOnRotationSpeed = 15f;
    
    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 20f;
    [SerializeField] private float rollStaminaCost = 25f;
    [SerializeField] private float runStaminaDrainRate = 15f;
    
    private CharacterController characterController;
    private PlayerCamera cameraController;
    private Animator anim;
    private PlayerCombat playerCombat;

    private Vector3 movementVelocity;
    private float verticalVelocity;
    private Vector3 rollDirection;
    private float currentStamina;
    private float currentRollSpeed;
    private bool isRolling;
    private float rollTimeRemaining;
    private bool isMoving;
    private bool isRunning;

    //Animation parameters (better performance this way)
    private readonly int forwardSpeedHash = Animator.StringToHash("ForwardSpeed");
    private readonly int rightSpeedHash = Animator.StringToHash("RightSpeed");
    private readonly int isRunningHash = Animator.StringToHash("IsRunning");
    private readonly int rollForwardHash = Animator.StringToHash("RollForward");
    private readonly int rollBackwardHash = Animator.StringToHash("RollBackward");
    private readonly int rollLeftHash = Animator.StringToHash("RollLeft");
    private readonly int rollRightHash = Animator.StringToHash("RollRight");
    private readonly int runningRollHash = Animator.StringToHash("IsRunningRoll");
    
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        cameraController = Camera.main.GetComponent<PlayerCamera>();
        anim = GetComponent<Animator>();
        playerCombat = GetComponent<PlayerCombat>();
        
        if (cameraController != null)
            cameraController.SetTarget(transform);

        currentStamina = maxStamina;
        currentRollSpeed = rollSpeed;
    }
    
    private void Update()
    {
        HandleMovement();
        HandleRolling();
        HandleStamina();
        ApplyGravity();
        UpdateAnimations();
    }
    
    private void HandleMovement()
    {
        if (isRolling || (playerCombat != null && playerCombat.IsAttacking())) return;
        
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool wantsToRun = Input.GetKey(KeyCode.LeftShift);
        
        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 moveDirection = GetMoveDirection(input);
        
        float targetSpeed = 0f;
        if (input.magnitude > 0.1f)
        {
            isMoving = true;
            isRunning = wantsToRun && currentStamina > 0;
            targetSpeed = isRunning ? runSpeed : walkSpeed;
        }
        else
        {
            isMoving = false;
            isRunning = false;
        }
        
        movementVelocity = moveDirection * targetSpeed;
        Vector3 motion = movementVelocity * Time.deltaTime;
        motion.y = verticalVelocity * Time.deltaTime;
        characterController.Move(motion);
        
        if (moveDirection.magnitude > 0.1f)
        {
            HandleRotation(moveDirection);
        }
    }
    
    private Vector3 GetMoveDirection(Vector3 input)
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();
        
        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();
        
        return (cameraForward * input.z + cameraRight * input.x).normalized;
    }
    
    private void HandleRotation(Vector3 direction)
    {
        if (cameraController != null && cameraController.IsLockedOn())
        {
            Transform target = cameraController.GetLockOnTarget();
            if (target != null)
            {
                Vector3 directionToTarget = (target.position - transform.position).normalized;
                directionToTarget.y = 0f;
                if (directionToTarget.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                        Time.deltaTime * lockOnRotationSpeed);
                }
            }
        }
        else
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 
                Time.deltaTime * rotationSpeed);
        }
    }
    
    private void HandleRolling()
    {
        if(playerCombat != null && playerCombat.IsInCommittedAttack()) return;

        if (Input.GetKeyDown(KeyCode.Space) && !isRolling && currentStamina >= rollStaminaCost)
        {
            isRolling = true;
            rollTimeRemaining = rollDuration;
            currentStamina -= rollStaminaCost;
            
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;
            
            if (input.magnitude > 0.1f)
            {
                rollDirection = GetMoveDirection(input);
            }
            else
            {
                rollDirection = transform.forward;
            }

            // Set whether this is a running roll or not
            anim.SetBool(runningRollHash, isRunning);
            TriggerRollAnimation(input);

            if (isRunning)
            {
                currentRollSpeed *= 1.35f;
            }
        }
        
        if (isRolling)
        {
            Vector3 motion = rollDirection * currentRollSpeed * Time.deltaTime;
            motion.y = verticalVelocity * Time.deltaTime;
            characterController.Move(motion);
            
            rollTimeRemaining -= Time.deltaTime;
            if (rollTimeRemaining <= 0f)
            {
                isRolling = false;
                anim.SetBool(runningRollHash, false);
                currentRollSpeed = rollSpeed; 
            }
        }
    }

    private void TriggerRollAnimation(Vector3 input)
    {
        // Determine roll direction based on input
        if (Mathf.Abs(input.z) > Mathf.Abs(input.x))
        {
            if (input.z > 0)
                anim.SetTrigger(rollForwardHash);
            else
                anim.SetTrigger(rollBackwardHash);
        }
        else
        {
            if (input.x > 0)
                anim.SetTrigger(rollRightHash);
            else
                anim.SetTrigger(rollLeftHash);
        }
    }

    private void ResetRollAnimations()
    {
        anim.ResetTrigger(rollForwardHash);
        anim.ResetTrigger(rollBackwardHash);
        anim.ResetTrigger(rollLeftHash);
        anim.ResetTrigger(rollRightHash);
    }

    private void UpdateAnimations()
    {
        if (!isRolling)
        {
            // Convert world space velocity to local space for animation
            Vector3 localVelocity = transform.InverseTransformDirection(movementVelocity);
            
            // Normalize speeds between -1 and 1 for blend tree
            float normalizedForwardSpeed = localVelocity.z / (isRunning ? runSpeed : walkSpeed);
            float normalizedRightSpeed = localVelocity.x / (isRunning ? runSpeed : walkSpeed);
            
            // Update animator parameters
            anim.SetFloat(forwardSpeedHash, normalizedForwardSpeed, 0.1f, Time.deltaTime);
            anim.SetFloat(rightSpeedHash, normalizedRightSpeed, 0.1f, Time.deltaTime);
            anim.SetBool(isRunningHash, isRunning);
        }
    }
    
    private void HandleStamina()
    {
        if (isRunning && isMoving)
        {
            currentStamina = Mathf.Max(0f, currentStamina - runStaminaDrainRate * Time.deltaTime);
        }
        else if (!isRolling)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
        }
    }
    
    private void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }
    
    public bool IsRolling() => isRolling;
    public bool IsRunning() => isRunning;
    public bool IsMoving() => isMoving;
    public float GetStaminaPercentage() => currentStamina / maxStamina;
}