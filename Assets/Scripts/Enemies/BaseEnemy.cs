using UnityEngine;
using UnityEngine.AI;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected float maxHealth = 100f;
    protected float currentHealth;

    [Header("Movement Settings")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float detectionRange = 10f;
    [SerializeField] protected float attackRange = 2f;
    
    protected Transform player;
    protected NavMeshAgent agent;
    protected Animator animator;
    protected bool isAttacking = false;
    
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange;
        }
    }
    
    protected virtual void Update()
    {
        if (player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange && !isAttacking)
            {
                StartAttackSequence();
            }
            else if (!isAttacking)
            {
                MoveTowardsPlayer();
            }
        }
    }
    
    public virtual void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Trigger hit animation/effects
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
        }
    }
    
    protected virtual void Die()
    {
        // Base death behavior
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        // Disable components
        if (agent != null) agent.enabled = false;
        enabled = false;
        
        // Optional: Add death effects, drop items, etc.
        Destroy(gameObject, 3f); // Destroy after death animation
    }
    
    protected virtual void MoveTowardsPlayer()
    {
        if (agent != null && agent.enabled)
        {
            agent.SetDestination(player.position);
            if (animator != null)
            {
                animator.SetBool("IsMoving", agent.velocity.magnitude > 0.1f);
            }
        }
    }
    
    protected abstract void StartAttackSequence();
}