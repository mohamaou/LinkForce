using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Core;
using DG.Tweening; // Make sure DOTween is installed and configured.

namespace Troops
{
    public class HealthBar : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Image[] images;
        [SerializeField] private Image fillImage; // Assign the slider's fill image here.
        
        private float _maxHealth;
        private float _currentHealth;
        private bool _isPlayer, _permanentHide;

        [Header("Damage Feedback")]
        [SerializeField] private float damageDisplayTime = 1f; // How long images stay visible after damage

        private void Start()
        {
            SetImagesVisible(false);
            healthBar.value = 1;
        }

        public void SetHealthBar(float maxHealth, bool isPlayer)
        {
            _maxHealth = maxHealth;
            _currentHealth = _maxHealth;
            _isPlayer = isPlayer;

            // Set up slider
            healthBar.minValue = 0;
            healthBar.maxValue = _maxHealth;
            healthBar.value = _maxHealth;

            // Set fill color based on player/enemy
            fillImage.color = _isPlayer ? GameManager.Instance.player1Color : GameManager.Instance.player2Color;

            // Hide images at initialization
            SetImagesVisible(false);
        }

        public void SetHealth(float health)
        {
            // Clamp health so it never goes below 0 or above max
            _currentHealth = Mathf.Clamp(health, 0, _maxHealth);

            // Animate the slider value using DOTween
            healthBar.DOValue(_currentHealth, 0.5f).SetEase(Ease.OutQuad);
            SetImagesVisible(true);
            StopAllCoroutines(); 
        }

        public void TakeDamage(float damage)
        {
            _currentHealth -= damage;
            if (_currentHealth < 0f) _currentHealth = 0f;

            // Update slider with animation
           
            healthBar.value = _currentHealth;
            
            SetImagesVisible(true);
            StopAllCoroutines(); 
            //StartCoroutine(HideImagesAfterDelay(damageDisplayTime));
        }

        public void PermanentHide()
        {
            SetImagesVisible(false);
            _permanentHide = true;
        }
        private void SetImagesVisible(bool visible)
        {
            if (_permanentHide || !enabled) return;
            for (int i = 0; i < images.Length; i++)
            {
                images[i].enabled = visible;
            }
        }

        private IEnumerator HideImagesAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SetImagesVisible(false);
        }
    }
}