using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RandomInitialVelocity : MonoBehaviour
{
    [SerializeField][Range(0,10)] float minRandomSpeedForward;
    [SerializeField][Range(0,10)] float maxRandomSpeedForward;
    [SerializeField][Range(0,360)] float randomSpeedCone;

    private Rigidbody2D rb2D;
    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        ApplyRandomStartingSpeed();
    }
    private void OnValidate()
    {
        if(minRandomSpeedForward > maxRandomSpeedForward) maxRandomSpeedForward = minRandomSpeedForward;
    }
    private void ApplyRandomStartingSpeed()
    {
        float randomDirection = Random.Range(-(randomSpeedCone/2), randomSpeedCone/2);
        float randomSpeed = Random.Range(0, maxRandomSpeedForward);

        if (rb2D == null)
        {
            return;
        }
        rb2D.velocity += GeometryUtils.AngleToDirectionVector(randomDirection + transform.rotation.eulerAngles.z) * randomSpeed;
    }
}
