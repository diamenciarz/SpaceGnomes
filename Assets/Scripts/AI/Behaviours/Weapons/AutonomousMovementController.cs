using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AutonomousMovementController : ActivateOnSpawn
{
    public Func<float, float> VelocityFunction;
}
