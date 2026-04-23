using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AIControlInput))]
public class UnitAI : MonoBehaviour, ICommandable
{
    [Header("Defined Behaviors")]
    [SerializeField] MoveToPositionBehavior movementToPositionBehavior;

    [SerializeField] private MovementBehavior defaultMovementBehavior;
    private AIControlInput myAIControlInput;

    #region Public Methods
    #region ICommandable
    public void MoveTo(Vector2 position)
    {
        movementToPositionBehavior.SetTargetPosition(gameObject, position, this);
        myAIControlInput.movementBehavior = movementToPositionBehavior;
    }
    #endregion
    #region UnitAI
    public void NotifyReachedTargetPosition()
    {
        myAIControlInput.movementBehavior = defaultMovementBehavior;
    }
    #endregion
    #endregion
    private void Start()
    {
        myAIControlInput = GetComponent<AIControlInput>();
        defaultMovementBehavior = myAIControlInput.movementBehavior;
    }
}
