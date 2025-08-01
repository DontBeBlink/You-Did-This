using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PowerUp : MonoBehaviour
{
    public static readonly string ANIMATION_PICKUP = "pickup";

    public PowerUpType type;
    public float value;
    public bool playerOnly = true;
    public ParticleSystem powerUpParticles;
    public Vector2 powerUpOffset;
    public float destroyDelay;
    public Color color;

    // --- NEW: Custom PowerUp event ---
    public PowerUpCustomEvent customEvent; // Assign your custom script here

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        foreach (var p in GetComponentsInChildren<ParticleSystem>())
        {
            ParticleSystem.MainModule main = p.main;
            main.startColor = color;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (playerOnly && !other.GetComponent<PlayerController>())
        {
            return;
        }
        CharacterData character = other.GetComponent<CharacterData>();
        if (character)
        {
            switch (type)
            {
                case PowerUpType.MoveSpeed:
                    character.maxSpeed += value;
                    break;
                case PowerUpType.ExtraJump:
                    character.maxExtraJumps = Mathf.RoundToInt(value);
                    break;
                case PowerUpType.Dash:
                    character.canDash = true;
                    break;
                case PowerUpType.AirDash:
                    character.canDash = true;
                    character.maxAirDashes += Mathf.RoundToInt(value);
                    break;
                case PowerUpType.WallJump:
                    character.canWallSlide = true;
                    character.canWallJump = true;
                    break;
                case PowerUpType.Custom:
                    // --- NEW: Call custom event if assigned ---
                    if (customEvent != null)
                        customEvent.OnPowerUp(other.gameObject);
                    break;
                default:
                    break;
            }
        }
        animator.SetTrigger(ANIMATION_PICKUP);
        other.GetComponent<Animator>().SetTrigger(ANIMATION_PICKUP);
        powerUpParticles.transform.position = other.transform.position + (Vector3)powerUpOffset;
        powerUpParticles.transform.SetParent(other.transform);
        ParticleSystem.MainModule main2 = powerUpParticles.main;
        main2.startColor = color;
        powerUpParticles.Play();
        other.GetComponentInChildren<SpriteRenderer>().material.SetColor("_GlowColor", color);
        GetComponent<AudioSource>().Play();
        Destroy(powerUpParticles.gameObject, destroyDelay);
        Destroy(gameObject, destroyDelay);
    }

    public enum PowerUpType
    {
        MoveSpeed,
        ExtraJump,
        Dash,
        AirDash,
        WallJump,
        Custom // --- NEW: Custom type ---
    }
}