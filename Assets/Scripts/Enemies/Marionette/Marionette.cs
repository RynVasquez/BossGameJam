using UnityEngine;
using System.Collections;

public class Marionette : BaseEnemy
{
    [Header("Marionette Attack Settings")]
    [SerializeField] private float attackCooldown = 2f;
    
    private bool canAttack = true;
    
    protected override void StartAttackSequence()
    {
        if (!canAttack) return;
        
    }

}