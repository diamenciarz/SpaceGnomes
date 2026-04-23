using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISettableTarget
{
    public struct TargetInstance
    {
        private GameObject targetObject;
        private Vector2 targetPosition;
        private bool usePosition;
        public TargetInstance(GameObject targetObject)
        {
            this.targetObject = targetObject;
            this.targetPosition = Vector2.zero;
            this.usePosition = false;
        }
        public TargetInstance(Vector2 targetPosition)
        {
            this.targetObject = null;
            this.targetPosition = targetPosition;
            this.usePosition = true;
        }
        public Vector2 GetPosition()
        {
            if (usePosition) return targetPosition;
            return targetObject.transform.position;
        }
        public GameObject GetTargetObject()
        {
            if (!usePosition) return targetObject;
            return null;
        }
    }
    public void SetTarget(GameObject target);
    public void SetTarget(Vector2 target);
    public TargetInstance? GetTarget();
    public void StopTargetting();
}
