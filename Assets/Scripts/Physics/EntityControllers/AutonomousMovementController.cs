using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This abstract class serves as a base for any movement controller that needs to operate autonomously, such as homing missiles or projectiles with specific movement patterns.
/// It activates upon spawning and requires derived classes to implement the VelocityFunction, which defines how the velocity changes over time.
/// This allows for flexible movement behaviors that can be easily customized by implementing different velocity functions in subclasses.
/// </summary>
public abstract class AutonomousMovementController : ActivateOnSpawn
{
    public abstract float VelocityFunction(float time);
}
