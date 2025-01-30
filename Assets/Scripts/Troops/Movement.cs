using System;
using System.Collections;
using Core;
using UnityEngine;


namespace Troops
{
    [RequireComponent(typeof(Troop))]
    public class Movement : MonoBehaviour
    {
        [SerializeField] private Troop troop;
        [SerializeField] private float speed = 6f;
        private Rigidbody _rb;


        private void Reset()
        {
            troop = GetComponent<Troop>();
        }

        private void Awake()
        {
            troop.SetDeathEvent(() => { Destroy(this); });
        }

        private void Start()
        {
            _rb = troop.GetRigidbody();
            StartCoroutine(BattleMovement());
        }

        private IEnumerator BattleMovement()
        {
            yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Battle);
            yield return new WaitUntil(() => troop.GetTroopStat() == TroopState.Moving);

            float rotationSpeed = 5f;

            while (troop.GetTroopStat() != TroopState.Death)
            {
                if (troop.GetTroopStat() == TroopState.Moving)
                {
                    var target = troop.GetClosestEnemy();
                    troop.GetAnimator().Move(target != null);
                    if (target != null && !troop.IsShocked())
                    {
                        var direction = (target.transform.position - _rb.position).normalized;
                        _rb.MovePosition(_rb.position + direction * ((troop.IsFreezed()? speed/2f:speed )* Time.fixedDeltaTime));

                        var lookRotation = Quaternion.LookRotation(direction);
                        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, lookRotation,
                            rotationSpeed * Time.fixedDeltaTime));
                    }
                }

                yield return new WaitForFixedUpdate();
            }
        }
    }
}