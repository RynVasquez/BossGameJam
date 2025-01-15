using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [System.Serializable]
    public class AttackData
    {
        public string name;
        public float duration = 0.8f;
        public float coyoteTimeWindow = 0.2f;
        public KeyCode input = KeyCode.None;
        [Tooltip("Set to -1 to use left mouse, -2 for right mouse")]
        public int mouseButton = 0;
    }

    private enum AttackPhase
    {
        None,
        CoyoteWindow,
        Committed
    }

    [Header("Attack Settings")]
    [SerializeField] private AttackData[] attacks;
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Animation Layer Weights")]
    [SerializeField] private float attackLayerWeight = 1f;
    [SerializeField] private float layerTransitionSpeed = 10f;
    
    private Animator animator;
    private PlayerMovement playerMovement;
    private bool isAttacking;
    private float currentAttackTime;
    private float targetLayerWeight;
    private AttackData currentAttack;
    private AttackPhase currentPhase = AttackPhase.None;
    
    private readonly int lightAttackHash = Animator.StringToHash("LightAttack");
    private readonly int heavyAttackHash = Animator.StringToHash("HeavyAttack");
    private readonly int specialAttackHash = Animator.StringToHash("SpecialAttack");
    private readonly int attackLayerIndex = 2;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        
        animator.SetLayerWeight(attackLayerIndex, 0f);
        targetLayerWeight = 0f;

        if (attacks == null || attacks.Length == 0)
        {
            attacks = new AttackData[]
            {
                new AttackData { name = "Light", mouseButton = -1, duration = 0.6f, coyoteTimeWindow = 0.2f },
                new AttackData { name = "Heavy", mouseButton = -2, duration = 1f, coyoteTimeWindow = 0.3f },
                new AttackData { name = "Special", input = KeyCode.F, duration = 1.2f, coyoteTimeWindow = 0.25f }
            };
        }
    }
    
    private void Update()
    {
        HandleAttackInput();
        UpdateAttackState();
        UpdateLayerWeight();
    }
    
    private void HandleAttackInput()
    {
        if (!isAttacking && !playerMovement.IsRolling())
        {
            foreach (var attack in attacks)
            {
                bool inputTriggered = false;

                if (attack.mouseButton == -1) inputTriggered = Input.GetMouseButtonDown(0);
                else if (attack.mouseButton == -2) inputTriggered = Input.GetMouseButtonDown(1);
                else if (attack.mouseButton >= 0) inputTriggered = Input.GetMouseButtonDown(attack.mouseButton);
                
                if (attack.input != KeyCode.None) inputTriggered |= Input.GetKeyDown(attack.input);

                if (inputTriggered)
                {
                    StartAttack(attack);
                    break;
                }
            }
        }
        
        // Handle dodge canceling only during coyote window
        if (Input.GetKeyDown(KeyCode.Space) && isAttacking)
        {
            if (currentPhase == AttackPhase.CoyoteWindow)
            {
                if (showDebugLogs) Debug.Log("Dodge cancel successful during coyote window");
                CancelAttack();
            }
            else if (currentPhase == AttackPhase.Committed)
            {
                if (showDebugLogs) Debug.Log($"Dodge cancel failed - attack committed at {currentAttackTime:F3}s");
            }
        }
    }
    
    private void StartAttack(AttackData attack)
    {
        isAttacking = true;
        currentAttackTime = 0f;
        currentAttack = attack;
        currentPhase = AttackPhase.CoyoteWindow;
        
        if (showDebugLogs) Debug.Log($"Starting {attack.name} attack. Coyote window: {attack.coyoteTimeWindow:F3}");
        
        switch (attack.name.ToLower())
        {
            case "light":
                animator.SetTrigger(lightAttackHash);
                break;
            case "heavy":
                animator.SetTrigger(heavyAttackHash);
                break;
            case "special":
                animator.SetTrigger(specialAttackHash);
                break;
        }
        
        targetLayerWeight = attackLayerWeight;
    }
    
    private void CancelAttack()
    {
        if (showDebugLogs) Debug.Log("Canceling attack");
        
        isAttacking = false;
        currentAttackTime = 0f;
        currentPhase = AttackPhase.None;
        
        animator.ResetTrigger(lightAttackHash);
        animator.ResetTrigger(heavyAttackHash);
        animator.ResetTrigger(specialAttackHash);
        
        targetLayerWeight = 0f;
        currentAttack = null;
    }
    
    private void UpdateAttackState()
    {
        if (!isAttacking || currentAttack == null) return;
        
        currentAttackTime += Time.deltaTime;
        
        // Update attack phases
        if (currentPhase == AttackPhase.CoyoteWindow && currentAttackTime > currentAttack.coyoteTimeWindow)
        {
            currentPhase = AttackPhase.Committed;
            if (showDebugLogs) Debug.Log("Attack committed - coyote window ended");
        }
        
        // Check if attack should end
        if (currentAttackTime >= currentAttack.duration)
        {
            if (showDebugLogs) Debug.Log($"Attack completed. Duration: {currentAttackTime:F3}");
            CancelAttack();
        }
    }
    
    private void UpdateLayerWeight()
    {
        float currentWeight = animator.GetLayerWeight(attackLayerIndex);
        float newWeight = Mathf.Lerp(currentWeight, targetLayerWeight, Time.deltaTime * layerTransitionSpeed);
        animator.SetLayerWeight(attackLayerIndex, newWeight);
    }

    public bool IsInCommittedAttack()
    {
        return isAttacking && currentPhase == AttackPhase.Committed;
    }
    
    public bool IsAttacking() => isAttacking;
    public float GetAttackProgress() => currentAttackTime / (currentAttack?.duration ?? 1f);
    public string GetCurrentAttackType() => currentAttack?.name ?? "None";
}