using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static GeometryUtils;
using static EntityTypeProperty;

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
    [SerializeField][Range(1,10)] int behaviorUpdatesPerSecond = 5;

    private Cache<Vector2> controlVectorCache;
    private Rigidbody2D myRigidbody2D;
    private EntityTeam myTeam;


    private void Awake()
    {
        myTeam = GetComponent<EntityTeam>();
        myRigidbody2D = GetComponentInParent<Rigidbody2D>();
        controlVectorCache = CacheManager.Instance.CreateCache<Vector2>(CacheBehavior.Interval, 1f / behaviorUpdatesPerSecond);
    }

    // Ignore bool iAmVehicularController, it's only used by the keyboard input
    public override float GetHorizontalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local, bool iAmVehicularController = false)
    {
        if (!controlVectorCache.isCached)
        {
            controlVectorCache.Set(movementBehavior.CalculateControlVector(CreateMovementBehaviorData()));
        }
        Debug.DrawRay(transform.position, controlVectorCache.Get(), Color.yellow);
        return mode == ControlVectorCoordinates.Local ? GeometryUtils.WorldCoordsToLocal(controlVectorCache.Get(), transform).x : controlVectorCache.Get().x;
    }

    public override float GetVerticalInput(ControlVectorCoordinates mode = ControlVectorCoordinates.Local, bool iAmVehicularController = false)
    {
        if (!controlVectorCache.isCached)
        {
            controlVectorCache.Set(movementBehavior.CalculateControlVector(CreateMovementBehaviorData()));
        }
        return mode == ControlVectorCoordinates.Local ? GeometryUtils.WorldCoordsToLocal(controlVectorCache.Get(), transform).y : controlVectorCache.Get().y;
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
}
