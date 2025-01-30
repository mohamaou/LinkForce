using System.Collections;
using System.Collections.Generic;
using Core;
using DG.Tweening;
using Players;
using UnityEngine;

namespace Troops
{
    public class Ghost : MonoBehaviour
    {
        private List<Transform> _ghosts = new List<Transform>();
        private PlayerTeam _team;
        private Damage _ghostDamage;

        private void Start()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                _ghosts.Add(transform.GetChild(i));
            }
            StartCoroutine(GhostAttack());
        }
        
        public void SetGhost(PlayerTeam team, Damage damage)
        {
            _team = team;
            _ghostDamage = damage;
            
        }

        private IEnumerator GhostAttack()
        {
            yield return new WaitUntil(() => TurnsManager.PlayState == PlayState.Battle);
            while (_ghosts.Count > 0)
            {
                yield return new WaitForSeconds(1.5f);
                var ghost = _ghosts[0];
                _ghosts.RemoveAt(0);
                yield return new WaitUntil(() => Target() != null);
                ghost.SetParent(null);
                foreach (var t in ghost.GetComponentsInChildren<Transform>())
                {
                    t.DOLocalRotate(Vector3.zero, 0.2f);
                }
                ghost.GetChild(0).eulerAngles = new Vector3(90, 0, 0);
                ShootGhost(ghost);
            }
        }

        private void ShootGhost(Transform ghost)
        {
            var target = Target();
            ghost.DOMove(target.transform.position + Vector3.up, 0.4f).OnComplete(() =>
            {
                Destroy(ghost.gameObject);
                target.TakeDamage(_ghostDamage);
            });
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
    }
}
