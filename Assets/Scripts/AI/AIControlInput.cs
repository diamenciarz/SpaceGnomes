using UnityEngine;

/// <summary>
/// Provides control input to the ship based on a specified MovementBehavior. 
/// It calculates the control vector at a fixed interval defined by behaviorUpdatesPerSecond and caches it for use in GetHorizontalInput and GetVerticalInput.
/// The control vector can be returned in either world or local coordinates.
/// </summary>
[RequireComponent(typeof(EntityTeam))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Trajectory))]
//[RequireComponent(typeof(ShipController))]
public class AIControlInput : ControlInput
{
    public MovementBehavior movementBehavior;
    [SerializeField][Range(1,10)] int behaviorUpdatesPerSecond = 5;

    private Cache<ControlInputData> controlInputDataCache;
    private Rigidbody2D myRigidbody2D;
    private EntityTeam myTeam;

    private void Awake()
    {
        myTeam = GetComponent<EntityTeam>();
        myRigidbody2D = GetComponentInParent<Rigidbody2D>();
        controlInputDataCache = CacheManager.Instance.CreateCache<ControlInputData>(CacheBehavior.Interval, 1f / behaviorUpdatesPerSecond);
    }
    /// <summary>
    /// Gets the control input data based on the cached control vector. If the cache is expired, it recalculates the control vector using the MovementBehavior and updates the cache.
    /// </summary>
    /// <param name="iAmVehicularController"></param>
    /// <returns></returns>
    public override ControlInputData GetControlInput(ControlVectorCoordinates mode, bool iAmVehicularController = false)
    {
        if (!controlInputDataCache.isCached)
        {
            controlInputDataCache.Set(movementBehavior.CalculateControlVector(CreateMovementBehaviorData()));
        }
        if (mode == ControlVectorCoordinates.World) return controlInputDataCache.Get();
        Vector2 localControlVector = GeometryUtils.WorldCoordsToLocal(controlInputDataCache.Get().controlVector, transform);
        return new ControlInputData(localControlVector, controlInputDataCache.Get().rotation, controlInputDataCache.Get().targetVelocity);
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
