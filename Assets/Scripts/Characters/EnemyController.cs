using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates { GUARD,PARROL,CHASE,DEAD}
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharacterStats))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{
    private EnemyStates enemyStates;
    private NavMeshAgent agent;
    private Animator anim;
    protected CharacterStats CharacterStats;
    private Collider coll;

    [Header("BasicSettings")]
    public float sightRadius;
    protected GameObject attackTarget;
    public bool isGuard;
    private float speed;
    public float lookAtTime;
    private float remainLookAtTime;
    private float lastAttackTime;
    private Quaternion guardRotation;

    [Header("Patrol State")]
    public float patrolRange;
    private Vector3 wayPoint;
    private Vector3 guardPos;

    bool isWalk, isChase, isFollow,isDead,playerDead;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        CharacterStats = GetComponent<CharacterStats>();
        coll = GetComponent<Collider>();

        speed = agent.speed;
        guardPos = transform.position;
        guardRotation = transform.rotation;
        remainLookAtTime = lookAtTime;
    }

    void Start()
    {
        if(isGuard)
        {
            enemyStates = EnemyStates.GUARD;
            GameManager.Instance.AddObserver(this);
        }
        else
        {
            enemyStates = EnemyStates.PARROL;
            GetNewWayPoint();
            //FIXME:�����л����޸ĵ�
            GameManager.Instance.AddObserver(this);
        }
    }

    //�л�����ʱʹ��
    //void OnEnable()
    //{
    //    GameManager.Instance.AddObserver(this);
    //}

    void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }

    private void Update()
    {
        if (CharacterStats.CurrrentHealth == 0)
            isDead = true;
        if(!playerDead)
        {
            SwitchStates();
            SwichAnimation();
            lastAttackTime -= Time.deltaTime;
        }
        
    }

    void SwichAnimation()
    {
        anim.SetBool("Walk", isWalk);
        anim.SetBool("Chase", isChase);  
        anim.SetBool("Follow", isFollow);
        anim.SetBool("Critical", CharacterStats.isCritical);
        anim.SetBool("Death", isDead);
    }

    void SwitchStates()
    {
        if (isDead)
            enemyStates = EnemyStates.DEAD;
        else if (FindPlayer())
        {
            enemyStates = EnemyStates.CHASE;
            //Debug.Log("�ҵ�Player");
        }

        switch(enemyStates)
        {
            case EnemyStates.GUARD:
                isChase = false;
                if(transform.position!=guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;
                    if (Vector3.SqrMagnitude(guardPos - transform.position) <= agent.stoppingDistance)
                    { 
                        isWalk = false;
                        transform.rotation = Quaternion.Lerp(transform.rotation, guardRotation, 0.01f);
                    }

                }
                break;
            case EnemyStates.PARROL:
                isChase = false;
                agent.speed = speed * 0.5f;

                //�ж��Ƿ񵽴����Ѳ�ߵ�
                if(Vector3.Distance(wayPoint,transform.position)<=agent.stoppingDistance)
                {
                    isWalk = false;
                    if (remainLookAtTime > 0)
                        remainLookAtTime -= Time.deltaTime;
                    else
                        GetNewWayPoint();
                }
                else
                {
                    isWalk = true;
                    agent.destination = wayPoint;
                }
                break;
            case EnemyStates.CHASE:
                //׷��Player
                isWalk = false;
                isChase = true;

                agent.speed = speed;
                if(!FindPlayer())
                {
                    //���ѻص���һ��״̬
                    isFollow = false;
                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else if (isGuard)
                        enemyStates = EnemyStates.GUARD;
                    else
                        enemyStates = EnemyStates.PARROL;

                }
                else
                {
                    isFollow = true;
                    agent.isStopped = false;
                    agent.destination = attackTarget.transform.position;
                }
                //�ڹ�����Χ�ڹ���
                if(TargetInAttackRange()||TargetInSkillRange())
                {
                    isFollow = false;
                    agent.isStopped = true;

                    if (lastAttackTime < 0)
                    {
                        lastAttackTime = CharacterStats.AttackData.coolDown;

                        //�����ж�
                        CharacterStats.isCritical = Random.value < CharacterStats.AttackData.criticalChance;
                        //ִ�й���
                        Attack();
                    }
                }
                break;
            case EnemyStates.DEAD:
                agent.radius = 0;
                coll.enabled = false;
                Destroy(gameObject, 2f);
                break;
        }
    }

    void Attack()
    {
        transform.LookAt(attackTarget.transform);
        if (TargetInAttackRange())
        {
            //������������
            anim.SetTrigger("Attack");
        }
        else if(TargetInSkillRange())
        {
            //������������
            anim.SetTrigger("Skill");
        }
        else
        {
            Debug.Log("attack error!");
        }
        
    }

    bool TargetInAttackRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= CharacterStats.AttackData.attackRange;
        else
            return false;
    }

    bool TargetInSkillRange()
    {
        if (attackTarget != null)
            return Vector3.Distance(attackTarget.transform.position, transform.position) <= CharacterStats.AttackData.skillRange;
        else
            return false;
    }

    bool FindPlayer()
    {
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach (var target in colliders)
        {
            if(target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }
        attackTarget = null;
        return false;
    }

    void GetNewWayPoint()
    {
        remainLookAtTime = lookAtTime;
        float randomX = Random.Range(-patrolRange, patrolRange);
        float randomZ = Random.Range(-patrolRange, patrolRange);

        Vector3 randomPoint = new Vector3(guardPos.x + randomX, transform.position.y, guardPos.z + randomZ);
       
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint, out hit, patrolRange, 1) ? hit.position : transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }

    //Animation Event
    void Hit()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            targetStats.TakeDamage(CharacterStats, targetStats);
        }
    }

    public void EndNotify()
    {
        //����
        //ֹͣ�����ƶ�
        //ֹͣAgent
        playerDead = true;
        anim.SetBool("Win", true);
        isChase = false;
        isWalk = false;
        isFollow = false;//
        attackTarget = null;
    }
    //TODO:��������
}
