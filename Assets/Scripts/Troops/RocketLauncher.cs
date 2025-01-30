using System.Collections;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using Players;
using UnityEngine;

namespace Troops
{
    public class RocketLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject trail;
        [SerializeField] private List<Transform> rocket;
        private Damage _damage;
        private PlayerTeam _team;
        private Vector3 _offset = new Vector3(-0.2f,0,-21.8f), _rotation = new Vector3(180,0,0);

        public void SetupRocket(PlayerTeam team, Damage damage)
        {
            _team = team;
            _damage = damage;
        }

        private void Start()
        {
            StartCoroutine(Attacking());
        }

        private IEnumerator Attacking()
        {
            yield return new WaitUntil(()=> TurnsManager.PlayState == PlayState.Battle);
            yield return new WaitForSeconds(1f);
            while (rocket.Count > 0)
            {
                yield return new WaitUntil(()=> Target() != null);
                var r = rocket[0];
                rocket.RemoveAt(0);
                r.SetParent(null);
                var t =Instantiate(trail, r);
                t.transform.localPosition = _offset;
                t.transform.localEulerAngles = _rotation;
                StartCoroutine(GoToEnemy(r, Target()));
                yield return new WaitForSeconds(2f);
            }
        }

        private IEnumerator GoToEnemy(Transform roc, Troop target)
        {
            float rotateDuration = 0.2f;
            float speed = 12f;
            Vector3 startPos = roc.position;
            Vector3 midPos = startPos + Vector3.up * 3f;
            Quaternion startRot = roc.rotation;
            Quaternion upRot = Quaternion.LookRotation(Vector3.up);
            float distance = Vector3.Distance(startPos, midPos);
            float travelTime = distance / speed;
            float elapsed = 0f;
            while (elapsed < travelTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / travelTime;
                float r = Mathf.Clamp01(elapsed / rotateDuration);
                roc.rotation = Quaternion.Slerp(startRot, upRot, r);
                roc.position = Vector3.Lerp(startPos, midPos, t);
                yield return null;
            }
            roc.position = midPos;
            roc.rotation = upRot;
            while (true)
            {
                Vector3 targetPos = target.transform.position + Vector3.up;
                Vector3 dir = targetPos - roc.position;
                float moveStep = speed * Time.deltaTime;
                if (dir.magnitude <= moveStep)
                {
                    roc.position = targetPos;
                    break;
                }
                Quaternion curRot = roc.rotation;
                Quaternion tgtRot = Quaternion.LookRotation(dir);
                roc.rotation = Quaternion.Slerp(curRot, tgtRot, Time.deltaTime / rotateDuration);
                roc.position += dir.normalized * moveStep;
                yield return null;
            }
            Destroy(roc.gameObject);
            target.TakeDamage(_damage);
        }
        
        private Troop Target()
        {
            var enemies = FindObjectsOfType<Troop>();
            Troop closestEnemy =null;
            var closetDistance = float.MaxValue;
            foreach (var enemy in enemies)
            {
                if (enemy.gameObject.CompareTag(_team == PlayerTeam.Player1 ? "Player 2" : "Player 1") &&
                    enemy.gameObject.layer == 6)
                {
                    var dist = Vector3.Distance(transform.position,enemy.transform.position);
                    if (dist < closetDistance)
                    {
                        closestEnemy = enemy;
                        closetDistance = dist;
                    }
                }
            }

            return closestEnemy;
        }

        public void Stop()
        {
            StopAllCoroutines();
            enabled = false;
        }
    }
}
