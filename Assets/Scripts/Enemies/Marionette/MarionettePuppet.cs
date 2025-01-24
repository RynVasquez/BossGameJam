using UnityEngine;
using System.Collections;

public class MarionettePuppet : BaseEnemy
{
    [Header("Puppet Attack Settings")]
    [SerializeField] private float basicAttackDamage = 15f;
    [SerializeField] private float jumpAttackDamage = 25f;
    [SerializeField] private float jumpAttackRadius = 4f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    
    private bool canAttack = true;
    private Rigidbody rb;
    
    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
    }
    
    protected override void StartAttackSequence()
    {
        if (!canAttack) return;
        StartCoroutine(BasicAttack());
    }
    
    private IEnumerator BasicAttack()
    {
        isAttacking = true;
        canAttack = false;
        
        if (animator != null)
        {
            animator.SetTrigger("BasicAttack");
        }
        
        // Create attack hitbox
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position + transform.forward,
            attackRange
        );
        
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Player"))
            {
                col.SendMessage("TakeDamage", basicAttackDamage, SendMessageOptions.DontRequireReceiver);
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}