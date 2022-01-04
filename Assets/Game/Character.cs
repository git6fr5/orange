using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Params = GameRules.Params;

[RequireComponent(typeof(SpriteRenderer))]
public class Character : MonoBehaviour {

    SpriteRenderer spriteRenderer;

    public bool debugSelf;
    public bool debugMotion;

    public Params speed;
    public float damping;
    public float power;

    public bool step;
    public int stepShadowCount;
    public bool isStepping;
    public float stepDuration;
    public int stepSpeedMultiplier;

    private Vector3 velocity;
    private Vector3 acceleration;

    // Runs once before the first frame.
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Runs once every frame.
    void Update() {
        Input();
        Move();
        Step();
    }

    void Input() {

        if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) {
            step = true;
        }
        if (!step && !isStepping) {
            float horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            float vertical = UnityEngine.Input.GetAxisRaw("Vertical");
            acceleration = power * (new Vector3(horizontal, vertical, 0f).normalized);

            velocity += acceleration * Time.deltaTime;
            velocity = speed.Clamp(velocity.magnitude) * velocity.normalized;

            if (acceleration == Vector3.zero) {
                velocity *= damping;
            }
        }

    }

    void Move() {
        transform.position += velocity * Time.deltaTime;
    }

    void Step() {
        if (step) {
            // velocity = stepSpeed * velocity.normalized;
            Time.timeScale = 0.5f;
            velocity = stepSpeedMultiplier * velocity.magnitude * acceleration.normalized;
            isStepping = true;
            StartCoroutine(IEStep());
            step = false;
        }
    }

    IEnumerator IEStep() {
        for (int i = 0; i < stepShadowCount; i++) {
            LeaveShadow(stepDuration);
            yield return new WaitForSeconds(stepDuration / stepShadowCount);
        }
        isStepping = false;
        yield return new WaitForSeconds(stepDuration);
        Time.timeScale = 1f;
        yield return null;
    }

    SpriteRenderer LeaveShadow(float duration) {
        SpriteRenderer afterImage = new GameObject("After Image", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
        afterImage.sprite = spriteRenderer.sprite;
        afterImage.transform.position = transform.position;
        Destroy(afterImage.gameObject, duration);
        return afterImage;
    }

    // Debugging.
    void OnDrawGizmos() {

        if (debugSelf) {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }

        // Motion.
        if (debugMotion) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + velocity);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + velocity, transform.position + velocity + acceleration);
        }

    }

}
