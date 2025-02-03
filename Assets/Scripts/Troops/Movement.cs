using System;
using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.AI;


namespace Troops
{
    [RequireComponent(typeof(Troop))]
    public class Movement : MonoBehaviour
    {
        [SerializeField] private Troop troop;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private float speed = 3f;
        [SerializeField] private float spawnSpeed = 2f;


        private void Reset()
        {
            troop = GetComponent<Troop>();
        }

        private void Awake()
        {
            troop.SetDeathEvent(() =>
            {
                Destroy(agent);
                Destroy(this);
            });
        }

        private void Start()
        {
             StartCoroutine(BattleMovement());
        }

        private IEnumerator BattleMovement()
        {
            yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Battle);
            yield return new WaitUntil(() => troop.GetTroopStat() == TroopState.Moving);

            float rotationSpeed = 5f;

            while (troop.GetTroopStat() != TroopState.Death)
            {
                if (troop.GetTroopStat() == TroopState.Moving && !troop.IsShocked())
                {
                    agent.isStopped = false;
                    var target = troop.GetClosestEnemy();
                    troop.GetAnimator().Move(target != null);
                    if (target != null && !troop.IsShocked())
                    {
                        agent.speed = troop.IsFreezed() ? speed / 2f : speed;
                        var direction = (target.transform.position - transform.position).normalized;
                        agent.SetDestination(target.transform.position);
                        var lookRotation = Quaternion.LookRotation(direction);
                        transform.rotation = (Quaternion.Slerp(transform.rotation, lookRotation,
                            rotationSpeed * Time.fixedDeltaTime));
                    }
                }
                else
                {
                    troop.GetAnimator().Move(false);
                    agent.isStopped = true;
                }
                
                yield return new WaitForFixedUpdate();
            }
        }
        
    }
}