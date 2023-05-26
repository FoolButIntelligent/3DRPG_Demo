using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public enum RockStates { Hitplayer,HitEnemy,HitNothing}
    private Rigidbody rb;
    public RockStates rockStates;

    [Header("BasicSettings")]
    public float force;
    public int damage;
    public GameObject target;
    private Vector3 direction;
    public GameObject breakEffect;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one;
        FlySToTarget();
    }

    private void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude<1f)
        {
            rockStates = RockStates.HitNothing;
        }
    }

    public void FlySToTarget()
    {
        if (target == null)
            target = FindObjectOfType<PlayerController>().gameObject;
        direction = (target.transform.position - transform.position+Vector3.up).normalized;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }

    //change rock's 3 kinds of state
    private void OnCollisionEnter(Collision collision)
    {
        switch (rockStates)
        {
            case RockStates.Hitplayer:
                if (collision.gameObject.CompareTag("Player"))
                {
                    collision.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    collision.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force;
                    collision.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    collision.gameObject.GetComponent<CharacterStats>().TakeDamage(damage, collision.gameObject.GetComponent<CharacterStats>());

                    rockStates = RockStates.HitNothing;
                }

                break;
            case RockStates.HitEnemy:
                if (collision.gameObject.GetComponent<Golem>())
                {
                    var otherStats = collision.gameObject.GetComponent<CharacterStats>();
                    otherStats.TakeDamage(damage, otherStats);
                    Instantiate(breakEffect,transform.position,Quaternion.identity);
                    Destroy(gameObject);
                }
                break;
            case RockStates.HitNothing:

                break;
        }
    }
}
