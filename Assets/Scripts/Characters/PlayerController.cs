using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private GameObject attackTarget;
    private float lastAttackTime;
    private CharacterStats CharacterStats;
    private bool isDead;
    private float stopDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        CharacterStats = GetComponent<CharacterStats>();
        stopDistance = agent.stoppingDistance;
    }

    private void Start()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RigisterPlayer(CharacterStats);
    }

    private void Update()
    {
        isDead = CharacterStats.CurrrentHealth == 0;
        if (isDead)
            GameManager.Instance.NotifyObserver();
        SwitchAnimation();

        lastAttackTime -= Time.deltaTime;
        
    }

    private void SwitchAnimation()
    {
        animator.SetFloat("Speed", agent.velocity.sqrMagnitude);
        animator.SetBool("Death", isDead);
    }
    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if (isDead) return;
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;
        agent.destination = target;
    }
    private void EventAttack(GameObject target)
    {
        if (isDead) return;
        if (target != null)
        {
            attackTarget = target;
            CharacterStats.isCritical = UnityEngine.Random.value < CharacterStats.AttackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    //TODO:修改攻击范围参数
    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        agent.stoppingDistance = CharacterStats.AttackData.attackRange;

        transform.LookAt(attackTarget.transform);
        //transform.rotation = Quaternion.Lerp(transform.rotation, attackTarget.transform.rotation, 0.01f);
        while (Vector3.Distance(attackTarget.transform.position,transform.position)>CharacterStats.AttackData.attackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;//下一帧再次执行while
        }

        agent.isStopped = true;

        if (lastAttackTime < 0)
        {
            animator.SetBool("Critical", CharacterStats.isCritical);
            animator.SetTrigger("Attack");
            //重置冷却时间
            lastAttackTime = CharacterStats.AttackData.coolDown;
        }
    }

    //Animation Event
    void Hit()
    {
        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HitNothing)
            {
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 20, ForceMode.Impulse);
            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(CharacterStats, targetStats);
        }
        
    }

}
