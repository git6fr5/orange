using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour {

    public bool debugSelf;
    public bool fullFire;
    public int activateButton;
    private int retractButton;

    public Vector3[] joints;
    private Vector3[] prevJoints;

    public int extensions;
    public int maxExtensions;
    public Vector3 velocity;
    public float damping;
    // Vector3 hand;

    public float fireSpeed;
    public float fireCooldown;
    public bool isFiring;
    public float retractSpeed;
    public bool isRetracting;

    // Start is called before the first frame update
    void Start() {

        joints = new Vector3[maxExtensions];
        for (int i = 0; i < maxExtensions; i++) {
            joints[i] = Vector3.zero;
        }
        prevJoints = joints;

        extensions = -1;
        retractButton = (activateButton + 1) % 2;
    }

    // Runs once every frame.
    void Update() {

        if (!isFiring) {
            velocity *= damping;
            if (velocity.sqrMagnitude < 0.001f * 0.001f) {
                velocity = Vector3.zero;
            }
        }

        if (isRetracting) {

            if (extensions > 0) {
                velocity = joints[extensions - 1] - joints[extensions];
            }
            else {
                velocity = transform.localPosition - joints[0];
            }

            if (velocity.sqrMagnitude < 0.05f * 0.05f) {
                extensions -= 1;
            }
            velocity = velocity.normalized * retractSpeed;

            if (extensions < 0) {
                isRetracting = false;
                extensions = -1;
                velocity = Vector3.zero;
            }
        }

        if (Input.GetMouseButtonDown(activateButton)) {
            extensions += 1;
            if (extensions == maxExtensions) {
                Retract();
            }
            else {
                Fire();
            }
        }

        if (Input.GetMouseButtonDown(retractButton)) {
            extensions += 1;
            Retract();
        }

        Move();
        // Simulate();
    }

    void Simulate() {

        for (int i = 0; i < joints.Length; i++) {
            Vector3 velocity = joints[i] - prevJoints[i];
            prevJoints[i] = joints[i];
            joints[i] += velocity;
        }

        for (int i = 0; i < 20; i++) {
            Constraints(joints, fireSpeed * fireCooldown);
        }
    }

    void Constraints(Vector3[] segments, float length) {
        for (int i = 1; i < segments.Length; i++) {
            // Get the distance and direction between the segments.
            float newDist = (segments[i - 1] - segments[i]).magnitude;
            Vector3 direction = (segments[i - 1] - segments[i]).normalized;

            // Get the error term.
            float error = newDist - length;
            Vector3 errorVector = direction * error;

            // Adjust the segments by the error term.
            if (i != 1) {
                segments[i - 1] -= errorVector * 0.5f;
            }
            segments[i] += errorVector * 0.5f;
        }
    }

    void Move() {

        if (extensions < 0) {
            return;
        }
        for (int i = extensions; i < maxExtensions; i++) {
            joints[i] += velocity * Time.deltaTime;
        }

    }

    void Retract() {
        isRetracting = true;
        extensions -= 1;
    }

    void Fire() {

        if (isFiring) {
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 target = mousePos - (transform.position);
        if (extensions > 0) {
            target = mousePos - (joints[extensions - 1] + transform.position);
        }
        target.z = 0f;

        velocity = target.normalized * fireSpeed;

        isFiring = true;
        StartCoroutine(IEFire());
    }

    IEnumerator IEFire() {
        if (fullFire) {
            while (extensions < maxExtensions) {
                yield return new WaitForSeconds(fireCooldown);
                velocity *= 1.05f; // (Vector3)Random.insideUnitCircle;
                extensions += 1;
            }
            extensions -= 1;
        }
        isFiring = false;
        yield return null;
    }

    // Debugging.
    void OnDrawGizmos() {
        if (debugSelf) {
            Gizmos.color = Color.white;
            if (joints == null) {
                return;
            }
            for (int i = 0; i < joints.Length; i++) {
                Gizmos.DrawWireSphere(joints[i] + transform.position, 0.25f);
                if (i > 0) {
                    Gizmos.DrawLine(joints[i] + transform.position, joints[i-1] + transform.position);
                }
                else {
                    Gizmos.DrawLine(transform.position, joints[0] + transform.position);
                }
            }
        }
    }

}
