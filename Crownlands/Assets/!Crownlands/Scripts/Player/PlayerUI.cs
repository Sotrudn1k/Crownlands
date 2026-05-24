using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    public TextMeshProUGUI debugText;
    TextMeshProUGUI killCountText;

    Slider healthSlider;
    Slider staminaSlider;

    Health health;
    Fighting fighting;
    KingdomMenuUI kingdomMenuUI;

    private void Awake()
    {
        var canvas = FindAnyObjectByType<PlayerCanvas>();
        killCountText = canvas.killCountText;
        staminaSlider = canvas.staminaSlider;
        fighting = GetComponent<Fighting>();
        healthSlider = canvas.healthSlider;
        health = GetComponent<Health>();
        debugText = canvas.debugText;
    }
    private void Start()
    {
        staminaSlider.maxValue = fighting.maxStamina;
        healthSlider.maxValue = health.MaxHealth;
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        killCountText.text = "Kills: " + fighting.killCount;
        healthSlider.value = health.currentHealth;
        staminaSlider.value = fighting.stamina;
    }
}
