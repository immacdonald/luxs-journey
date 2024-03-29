﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MidasPowerup : Powerup, IDestroyable {
    [Header("Visual")]
    private Transform visual;
    public float rotationSpeed;
    private float trueRotationSpeed;
    const float COLLECTION_TIME = 0.2f;
    private ParticleSystem destructionParticles;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
        visual = transform.GetChild(0);
        trigger = GetComponent<BoxCollider>();
        collider = GetComponent<CapsuleCollider>();
        rigidbody = GetComponent<Rigidbody>();
        destructionParticles = GetComponentInChildren<ParticleSystem>();
        SetPhysicsState(physicsObject);
    }

    private void Update() {
        visual.rotation = Quaternion.Euler(visual.rotation.x, (GameManager.GameTime * trueRotationSpeed) % 360, visual.rotation.z);
        //if (!physicsObject && !collected) visual.localPosition = new Vector3(startingPosition.x, Mathf.Lerp(startingPosition.y - movementAmount, startingPosition.y + movementAmount, Mathf.InverseLerp(-1, 1, Mathf.Sin(GameManager.GameTime * movementSpeed))), startingPosition.z);
    }

    public override void Pickup(Transform player) {
        base.Pickup(player);
        ApplyStatus(player.GetComponent<Player>());
        StartCoroutine(DestroyObject());
    }

    public IEnumerator DestroyObject() {
        destructionParticles.Emit(10);
        visual.gameObject.SetActive(false);
        yield return new WaitForSeconds(1.1f);
        Destroy(gameObject);
    }

    public override void SetPhysicsState(bool state) {
        base.SetPhysicsState(state);
        trueRotationSpeed = (360 / rotationSpeed);
    }

    public override void ApplyStatus(Player player) {
        player.cloakRenderer.material = capeMaterial;
        player.ApplyPowerup(powerupIdentifier, maxTime);
    }
}
