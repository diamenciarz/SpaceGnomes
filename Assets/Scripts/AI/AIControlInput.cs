using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static GeometryUtils;
using static EntityType;

[RequireComponent(typeof(EntityTeam))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Trajectory))]
//[RequireComponent(typeof(ShipController))]
public class AIControlInput : ControlInput
{
    public enum ControlVectorCoordinates
    {
        World,
        Local
    }

    [SerializeField] MovementBehavior movementBehavior;

    private Vector2 controlVector = Vector2.zero;
    private bool calculatedControlThisFrame = false;
    private Rigidbody2D myRigidbody2D;
    private EntityTeam myTeam;


    private void Awake()
    {
        myTeam = GetComponent<EntityTeam>();
        myRigidbody2D = GetComponentInParent<Rigidbody2D>();
    }

    // Ignore bool iAmVehicularController, it's only used by the keyboard input
    public override float GetHorizontalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local, bool iAmVehicularController = false)
    {
        if (!calculatedControlThisFrame) controlVector = movementBehavior.CalculateControlVector(CreateMovementBehaviorData());
        Debug.DrawRay(transform.position, controlVector, Color.yellow);
        return mode == ControlVectorCoordinates.Local ? GeometryUtils.WorldCoordsToLocal(controlVector, transform).x : controlVector.x;
    }

    public override float GetVerticalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local, bool iAmVehicularController = false)
    {
        if (!calculatedControlThisFrame) controlVector = movementBehavior.CalculateControlVector(CreateMovementBehaviorData());
        return mode == ControlVectorCoordinates.Local ? GeometryUtils.WorldCoordsToLocal(controlVector, transform).y : controlVector.y;
    }

    public override float GetRotationInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local, bool iAmVehicularController = false)
    {
        return 0f;
    }

    private MovementBehavior.MovementBehaviorData CreateMovementBehaviorData()
    {
        return new MovementBehavior.MovementBehaviorData()
        {
            transform = transform,
            gameObject = gameObject,
            myTeam = myTeam,
            myRigidbody2D = myRigidbody2D,
            myTrajectory = GetComponent<Trajectory>()
        };
    }
    void Update()
    {
        calculatedControlThisFrame = false;
    }
}
