using System;
using Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


namespace Players
{
    public class Base : MonoBehaviour
    {
        [Serializable]
        private struct BaseGFX
        {
            [SerializeField] private Age age;
            [SerializeField] private GameObject baseGfx;
            [SerializeField] private Transform targetPointsParent;

            public Age Age => age;
            public GameObject BaseGfx => baseGfx;
            public Vector3 ClosestPoint(Vector3 myPosition)
            {
                if (targetPointsParent == null || targetPointsParent.childCount == 0)
                {
                    return myPosition; 
                }

                Transform closestTransform = null;
                float minDistanceSqr = Mathf.Infinity;

                foreach (Transform child in targetPointsParent)
                {
                    float distanceSqr = (child.position - myPosition).sqrMagnitude;
                    if (distanceSqr < minDistanceSqr)
                    {
                        minDistanceSqr = distanceSqr;
                        closestTransform = child;
                    }
                }

                return closestTransform != null ? closestTransform.position : myPosition;
            }

        }
        [Serializable]
        private class BaseHealth
        {
            [SerializeField] private Age age;
            [SerializeField] private int health;
            private int _startHealth;
            
            public bool IsMyAge(Age targetAge) => age == targetAge;
            public void SetHealth(float currentHealth)
            {
                _startHealth = health;
                health = (int)(_startHealth * currentHealth);
            }

            public void TakeDamage(int damage)
            {
                health -= damage;
            }
            public float GetHealth() => health / (float)_startHealth;
        }
        [SerializeField] private PlayerTeam playerTeam;
        [SerializeField] private BaseHealth[] health;
        [SerializeField] private BaseGFX[] bases;
        private BaseHealth _currentHealth;
      
        private Slider _healthSlider;
        private BaseGFX _activeBase;

        private void Start()
        {
           // _currentHealth = GetBaseHealth(Player.Instance.GetCurrentAge());
            _currentHealth.SetHealth(1);
           // _healthSlider = UIManager.Instance.GetHealthSlider(playerTeam);
            _healthSlider.value = _currentHealth.GetHealth();
        }

        public void SetBase(Age age, bool transition = false)
        {
            if (_currentHealth != null)
            {
                var oldHealth = _currentHealth;
                _currentHealth = GetBaseHealth(age);
                _currentHealth.SetHealth(oldHealth.GetHealth());
            }
            var nexBase = bases[0];
            var currentBase = _activeBase;
            for (int i = 0; i < bases.Length; i++)
            {
                if (bases[i].Age == age)
                {
                    bases[i].BaseGfx.SetActive(true);
                    nexBase = bases[i];
                }
                else
                {
                    bases[i].BaseGfx.SetActive(false);
                }
            }

            if (!transition)
            {
                _activeBase = nexBase;
                return;
            }
            _activeBase.BaseGfx.SetActive(true);
            var endValue = 6f;
            var duration = 1.5f; 
            
            foreach (var oldBase in _activeBase.BaseGfx.GetComponentsInChildren<Renderer>())
            {
                Material mat = oldBase.material;
                mat.SetFloat("_Reverse", 0f);
                mat.SetFloat("_Height", 0);
                float startValue = mat.GetFloat("_Height");
                DOTween.To(() => startValue, x => {
                    mat.SetFloat("_Height", x);
                }, endValue, duration).SetEase(Ease.Linear).OnComplete(()=>
                {
                    currentBase.BaseGfx.SetActive(false);
                });
            }

            foreach (var newBase in nexBase.BaseGfx.GetComponentsInChildren<Renderer>())
            {
                Material mat = newBase.material;
                mat.SetFloat("_Reverse", 1.0f);
                float startValue = mat.GetFloat("_Height");
                
                DOTween.To(() => startValue, x => {
                    mat.SetFloat("_Height", x);
                }, endValue, duration).SetEase(Ease.Linear);
            }
            _activeBase = nexBase;
        }
        public Vector3 GetPosition(Vector3 myPosition)
        {
            return _activeBase.ClosestPoint(myPosition);
        }

        private BaseHealth GetBaseHealth(Age targetAge)
        {
            for (int i = 0; i < health.Length; i++)
            {
                if (health[i].IsMyAge(targetAge)) return health[i];
            }
            return new BaseHealth();
        }
        
        
        public void TakeDamage(float damage)
        {
            if(!enabled) return;
            _currentHealth.TakeDamage((int)damage);
            _healthSlider.DOValue(_currentHealth.GetHealth(), 0.1f);
        }
    }
}
