using KBCore.Refs;
using UnityEngine;
using UnityEngine.AI;

namespace RowdyRobots{
public class EnemyPatrol : MonoBehaviour
{
    private NavMeshAgent enemyAgent;
    private Animator animator;
    [SerializeField,Anywhere] public Transform PointA;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyAgent = GetComponent<NavMeshAgent>();
        enemyAgent.SetDestination(PointA.position);
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyAgent.remainingDistance <= enemyAgent.stoppingDistance)
        {
            animator.SetBool("isWalking", false);
        }
        else
        {
            animator.SetBool("isWalking", true);
        }
    }

    // void OnAnimatorMove()
    // {
    //     if (animator.GetBool("isWalking"))
    //     {
    //         enemyAgent.speed = (animator.deltaPosition / Time.deltaTime).magnitude;
    //     }
    // }
}
}