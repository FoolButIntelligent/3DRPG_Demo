using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Golem : EnemyController
{
    [Header("Skill")]
    public float kickForce = 25;
    public GameObject rockPrefab;
    public Transform handPos;

    public void KickOff()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharacterStats>();
            Vector3 direction = (attackTarget.transform.position - transform.position).normalized;

            targetStats.GetComponent<NavMeshAgent>().isStopped = false;
            targetStats.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            targetStats.GetComponent<NavMeshAgent>().ResetPath();
            targetStats.TakeDamage(CharacterStats, targetStats);
            Debug.Log("forceback");
        }
    }

    public void ThrowRock()
    {
        if (attackTarget != null)
        {
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            rock.GetComponent<Rock>().target = attackTarget;
        }
    }
}
