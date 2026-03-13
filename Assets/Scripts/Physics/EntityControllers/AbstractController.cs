using UnityEngine;

public class AbstractController : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] protected ShipControlInput mainShipControlInput = null;
    [SerializeField] protected ShipControlInput alternativeShipControlInput = null;
    [SerializeField] protected KeyCode alternativeControlKey = KeyCode.Space;

    protected Rigidbody2D rb2d;

    protected virtual void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (alternativeShipControlInput == null) alternativeShipControlInput = mainShipControlInput;
        //rb2d.inertia *= 3f;
    }
}
