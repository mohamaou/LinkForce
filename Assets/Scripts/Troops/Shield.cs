using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Troops
{
    public class Shield : MonoBehaviour
    {
        [SerializeField] private Rigidbody shield;
        private List<Rigidbody> _shields = new();


        public bool IsShielded()
        {
            if (_shields.Count == 0) return false;

            var shieldRb = _shields[0];
            shieldRb.transform.SetParent(null);
            Destroy(shieldRb.gameObject,5);
            _shields.RemoveAt(0);

            if (shieldRb != null)
            {
                shieldRb.isKinematic = false;
                var throwDirection = (Random.insideUnitSphere + Vector3.up).normalized;
                var forceMagnitude = Random.Range(3f, 6f);
                var torqueMagnitude = Random.Range(5f, 10f);

                shieldRb.AddForce(throwDirection * forceMagnitude, ForceMode.Impulse);
                shieldRb.AddTorque(Random.insideUnitSphere * torqueMagnitude, ForceMode.Impulse);
            }

            return true;
        }

        public void SetShields(int count)
        {
            float radius = 1.145f;
            float height = 1.19f;
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 shieldPosition = transform.position + new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);

                var newShield = Instantiate(shield, shieldPosition, Quaternion.identity);
                newShield.transform.SetParent(transform);

                newShield.transform.LookAt(transform.position + Vector3.up * height);
                newShield.transform.eulerAngles += new Vector3(0, 180, 0);
                _shields.Add(newShield);
            }
            Destroy(shield.gameObject);
        }
    }
}
