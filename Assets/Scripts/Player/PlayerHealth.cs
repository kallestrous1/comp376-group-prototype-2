using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] public int maxHealth = 20;
    [SerializeField] private float invincibilityDuration = 1f;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;

    [Header("Displays")]
    [SerializeField] private GameObject bloodDisplay;
    [SerializeField] private GameObject deathDisplay;

    [Header("Audio")]
    [SerializeField] private AudioClip playerDamageSound;
    [SerializeField] private AudioClip playerDeathSound;

    private int health;
    private float invincibilityTimer;
    private bool died = false;

    public int CurrentHealth => health;
    public bool IsInvincible => invincibilityTimer > 0;

    public event System.Action<int, int> OnHealthChanged;

    void Start()
    {
        health = maxHealth;
        UpdateUI();
    }

    private void Update()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
    }

    public void ResetHealth()
    {
        died = false;
        health = maxHealth;
        UpdateUI();
        OnHealthChanged?.Invoke(health, maxHealth);
    }

    public void changeHealth(int amount, string cause = "unknown")
    {
        if (IsInvincible && amount < 0) return;

        health = Mathf.Clamp(health + amount, 0, maxHealth);
        UpdateUI();

        if (amount < 0) invincibilityTimer = invincibilityDuration;

        if (health <= 0 && !died)
        {
            died = true;
            PlaySound(playerDeathSound);
            StartCoroutine(DeathRoutine());
        }
        else if (amount < 0)
        {
            PlaySound(playerDamageSound);
            if (bloodDisplay) StartCoroutine(BloodFlash());
        }

        OnHealthChanged?.Invoke(health, maxHealth);
    }

    private IEnumerator DeathRoutine()
    {
        if (deathDisplay) deathDisplay.SetActive(true);
        yield return new WaitForSeconds(1f);
        if (deathDisplay) deathDisplay.SetActive(false);
        GameManager.manager.RespawnPlayer();
    }

    private IEnumerator BloodFlash()
    {
        bloodDisplay.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        bloodDisplay.SetActive(false);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip && AudioManager.Instance)
        {
            AudioManager.Instance.Play(clip);
        }
    }

    private void UpdateUI()
    {
        if (healthSlider)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }
    }
}
