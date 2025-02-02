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
        [SerializeField] private float speed = 3f;
        [SerializeField] private float spawnSpeed = 2f;
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
            // StartCoroutine(BattleMovement());
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

        public IEnumerator SpawnMovement(Transform troopTransform, Vector3 targetPosition)
        {
            troop.GetAnimator().Move(true);
            var direction = (targetPosition - troopTransform.position).normalized;
            troopTransform.rotation = Quaternion.LookRotation(direction);

            while (Vector3.Distance(troopTransform.position, targetPosition) > 0.1f)
            {
                troopTransform.position =
                    Vector3.MoveTowards(troopTransform.position, targetPosition, spawnSpeed * Time.deltaTime);

                direction = (targetPosition - troopTransform.position).normalized;
                troopTransform.rotation = Quaternion.LookRotation(direction);

                yield return null;
            }

            troopTransform.position = targetPosition;
            troop.GetAnimator().Move(false);

            var targetRotation = Quaternion.LookRotation(troopTransform.forward);
            while (Quaternion.Angle(troopTransform.rotation, targetRotation) > 1f)
            {
                troopTransform.rotation =
                    Quaternion.Slerp(troopTransform.rotation, targetRotation, Time.deltaTime * 5f);
                yield return null;
            }

            troopTransform.rotation = targetRotation;
        }
    }
}