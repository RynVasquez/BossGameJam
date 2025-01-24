using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryShooter : MonoBehaviour
{
    [SerializeField] GameObject projectilePrefab; // The object to fire
    [SerializeField] Transform launchPoint;       // Where the object will be fired from
    [SerializeField] Transform target;           // The target position
    [SerializeField] float launchAngle = 45f;    // The angle of the trajectory (in degrees)

    private void Start()
    {
        Fire();
    }
    public void Fire()
    {
        if (projectilePrefab == null || launchPoint == null || target == null)
        {
            Debug.LogError("Ensure projectilePrefab, launchPoint, and target are assigned.");
            return;
        }

        // Calculate the direction and distance to the target
        Vector3 targetPosition = target.position;
        Vector3 launchPosition = launchPoint.position;
        Vector3 toTarget = targetPosition - launchPosition;

        // Decompose distance into horizontal and vertical components
        float horizontalDistance = new Vector3(toTarget.x, 0, toTarget.z).magnitude;
        float verticalDistance = toTarget.y;

        // Convert launch angle to radians
        float angleRad = launchAngle * Mathf.Deg2Rad;

        // Calculate initial velocity required to reach the target
        float gravity = Physics.gravity.y; // Gravity is negative in Unity
        float velocitySquared = (gravity * horizontalDistance * horizontalDistance) /
                                (2 * (horizontalDistance * Mathf.Tan(angleRad) - verticalDistance));

        if (velocitySquared <= 0)
        {
            Debug.LogError("Target is out of range for the specified angle.");
            return;
        }

        float initialVelocity = Mathf.Sqrt(Mathf.Abs(velocitySquared));

        // Break initial velocity into components
        Vector3 velocity = new Vector3(toTarget.x, horizontalDistance * Mathf.Tan(angleRad), toTarget.z).normalized;
        velocity *= initialVelocity;

        // Instantiate and launch the projectile
        GameObject projectile = Instantiate(projectilePrefab, launchPosition, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = velocity;
        }
        else
        {
            Debug.LogError("Projectile prefab must have a Rigidbody component.");
        }
    }

    // Optional: Visualize trajectory in the scene view
    private void OnDrawGizmos()
    {
        if (launchPoint != null && target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(launchPoint.position, target.position);
        }
    }
}
