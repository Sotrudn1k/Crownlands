using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    public Slider healthSlider;
    public Slider staminaSlider;

    Health health;
    Fighting fighting;
    private void Awake()
    {
        health = GetComponent<Health>();
        fighting = GetComponent<Fighting>();
        var canvas = FindAnyObjectByType<PlayerCanvas>();
        healthSlider = canvas.healthSlider;
        staminaSlider = canvas.staminaSlider;
    }
    private void Start()
    {
        healthSlider.maxValue = health.MaxHealth;
        staminaSlider.maxValue = fighting.maxStamina;
    }
    private void Update()
    {
        if (!isLocalPlayer) return;
        healthSlider.value = health.currentHealth;
        staminaSlider.value = fighting.stamina;
    }
}
