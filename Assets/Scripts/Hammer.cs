using UnityEngine;

public class Hammer : MonoBehaviour
{
    [Header("Damage Values")]
    [SerializeField] private float lightAttackDamage = 25f;
    [SerializeField] private float heavyAttackDamage = 50f;
    [SerializeField] private float specialAttackDamage = 75f;
    
    [Header("Stamina Costs")]
    [SerializeField] private float lightAttackCost = 15f;
    [SerializeField] private float heavyAttackCost = 30f;
    [SerializeField] private float specialAttackCost = 45f;
    
    private PlayerCombat playerCombat;
    private PlayerMovement playerMovement;
    
    void Start()
    {
        playerCombat = GetComponentInParent<PlayerCombat>();
        playerMovement = GetComponentInParent<PlayerMovement>();
        
        if (playerCombat == null || playerMovement == null)
        {
            Debug.LogError("Hammer: Required player components not found!");
            enabled = false;
            return;
        }
    }
    
    void Update()
    {
        CheckStaminaForAttacks();
    }
    
    private void CheckStaminaForAttacks()
    {
        if (!playerCombat.IsAttacking()) return;
        
        float requiredStamina = GetStaminaCostForAttack(playerCombat.GetCurrentAttackType());
        
        if (playerMovement.GetStaminaPercentage() * 100f < requiredStamina)
        {
            if (!playerCombat.IsInCommittedAttack())
            {
                Debug.Log($"Insufficient stamina for {playerCombat.GetCurrentAttackType()} attack");
                playerCombat.SendMessage("CancelAttack", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
    
    private float GetStaminaCostForAttack(string attackType)
    {
        switch (attackType.ToLower())
        {
            case "light": return lightAttackCost;
            case "heavy": return heavyAttackCost;
            case "special": return specialAttackCost;
            default: return 0f;
        }
    }

    private float GetDamageForAttack(string attackType)
    {
        switch (attackType.ToLower())
        {
            case "light": return lightAttackDamage;
            case "heavy": return heavyAttackDamage;
            case "special": return specialAttackDamage;
            default: return 0f;
        }
    }
    
    void OnCollisionEnter(Collision other)
    {
        if (!playerCombat.IsAttacking()) return;

        string currentAttackType = playerCombat.GetCurrentAttackType();
        float staminaCost = GetStaminaCostForAttack(currentAttackType);
        float damage = GetDamageForAttack(currentAttackType);
        
        switch (other.gameObject.tag)
        {
            case "Marionette":
                var marionette = other.gameObject.GetComponent<Marionette>();
                if (marionette != null)
                {
                    playerMovement.SendMessage("ConsumeStamina", staminaCost, SendMessageOptions.DontRequireReceiver);
                    marionette.TakeDamage(damage);
                }
                break;

            case "MarionettePuppet":
                var puppet = other.gameObject.GetComponent<MarionettePuppet>();
                if (puppet != null)
                {
                    playerMovement.SendMessage("ConsumeStamina", staminaCost, SendMessageOptions.DontRequireReceiver);
                    puppet.TakeDamage(damage);
                }
                break;
        }
    }
}