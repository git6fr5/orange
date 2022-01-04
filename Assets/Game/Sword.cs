using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour {

    public Transform handle;
    public Vector3 handlePosition;
    public float handleLength;
    public float angle;
    public float rotationSpeed;

    public int constraintDepth;

    public LineRenderer edgeRenderer;
    public float swordBaseWidth;
    public float swordTipWidth;
    public float swordLength;

    public int edgeSegmentCount;
    public Vector3[] edgeSegments;
    public Vector3[] prevEdgeSegments;

    public int trailSegmentCount;
    public Vector3[] trailSegments;
    public float trailLength; // Vary based on speed.

    // Runs once before the first frame.
    void Start() {

        edgeSegments = new Vector3[edgeSegmentCount];
        edgeSegments[0] = transform.position;

        float length = swordLength / edgeSegmentCount;
        for (int i = 1; i < edgeSegments.Length; i++) {
            edgeSegments[i] = edgeSegments[i - 1] + Vector3.right * length;
        }

        prevEdgeSegments = edgeSegments;

        trailSegments = new Vector3[trailSegmentCount];
        for (int i = 0; i < trailSegmentCount; i++) {
            trailSegments[i] = Vector3.zero;
        }
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetKey(KeyCode.J)) {
            angle += (rotationSpeed * Time.deltaTime);
            if (angle > 360) {
                angle = 360 - angle;
            }
        }

        if (Input.GetKey(KeyCode.K)) {
            angle -= (rotationSpeed * Time.deltaTime);
            if (angle < 0) {
                angle = 360 + angle;
            }
        }

    }

    void FixedUpdate() {
        Handle();
        Edge();
        Trail();
        // Render();
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.white;
        for (int i = 1; i < edgeSegments.Length; i++) {
            Gizmos.DrawLine(edgeSegments[i - 1], edgeSegments[i]);
        }
        Gizmos.color = Color.yellow;
        for (int i = 1; i < trailSegments.Length; i++) {
            Gizmos.DrawLine(trailSegments[i - 1], trailSegments[i]);
        }
    }

    private void Render() {

        edgeRenderer.startWidth = swordBaseWidth;
        edgeRenderer.endWidth = swordTipWidth;
        edgeRenderer.positionCount = edgeSegmentCount;
        edgeRenderer.SetPositions(edgeSegments);
    }

    void Handle() {
        handlePosition = handle.position + Quaternion.Euler(0f, 0f, angle) * Vector3.right * handleLength;
    }

    void Edge() {
        for (int i = 0; i < edgeSegmentCount; i++) {
            //Vector3 velocity = edgeSegments[i] - prevEdgeSegments[i];
            //prevEdgeSegments[i] = edgeSegments[i];
            //edgeSegments[i] += velocity;
        }

        for (int i = 1; i < edgeSegmentCount; i++) {

            //Vector3 displacement = edgeSegments[i - 1] - edgeSegments[i];
            //float edgeAngle = Vector2.SignedAngle(Vector2.right, (Vector2)displacement);
            //edgeAngle = edgeAngle < 0f ? 360 + edgeAngle : angle;
            //float rotation = 0.05f * -(edgeAngle - angle);
            //edgeSegments[i] = edgeSegments[i - 1] + Quaternion.Euler(0f, 0f, rotation) * displacement;
        }

        float length = swordLength / edgeSegmentCount;
        for (int i = 0; i < constraintDepth; i++) {
            // Constraints(handlePosition, edgeSegments, length);
        }

        edgeSegments[0] = handlePosition;
        for (int i = 1; i < edgeSegmentCount; i++) {
            edgeSegments[i] = edgeSegments[i-1] + Quaternion.Euler(0f, 0f, angle) * Vector3.right * length;
        }

    }

    void Trail() {
        trailSegments[0] = edgeSegments[edgeSegments.Length -1];
        float length = trailLength / trailSegmentCount;
        for (int i = 1; i < trailSegments.Length; i++) {
            if ((trailSegments[i - 1] - trailSegments[i]).magnitude > length) {
                trailSegments[i] += (trailSegments[i - 1] - trailSegments[i]).normalized * Time.deltaTime * 5f;
            }
        }

        for (int i = 0; i < constraintDepth; i++) {
            Constraints(edgeSegments[edgeSegments.Length - 1], trailSegments, length);
        }
    }

    void Constraints(Vector3 origin, Vector3[] segments, float length) {
        segments[0] = origin;
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
        segments[0] = origin;
    }
}
