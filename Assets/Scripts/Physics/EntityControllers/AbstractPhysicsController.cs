using UnityEngine;

/// <summary>
/// AbstractPhysicsController is an abstract base class for physics
/// </summary>
public abstract class AbstractPhysicsController : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] protected ControlInput mainShipControlInput = null;
    [SerializeField] protected ControlInput alternativeShipControlInput = null;
    [SerializeField] protected KeyCode alternativeControlKey = KeyCode.Space;

    protected Rigidbody2D rb2d;

    protected virtual void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (alternativeShipControlInput == null) alternativeShipControlInput = mainShipControlInput;
        //rb2d.inertia *= 3f;
    }
}
