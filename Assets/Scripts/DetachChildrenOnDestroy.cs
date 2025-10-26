using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DetachChildrenOnDestroy : ActivateOnDespawn
{
    [System.Serializable]
    public struct ChildInfo
    {
        [Tooltip("Will be used to relatively split the total mass coming from parent among all children")] 
        public float relativeMass;
        public GameObject child;
    }

    [SerializeField] private List<ChildInfo> childrenToDetach;
    private float massToSplit;

    public void SetMassToSplit(float mass)
    {

        massToSplit = mass;
        float totalRelativeMass = childrenToDetach.Sum(childInfo => childInfo.relativeMass);
        foreach (ChildInfo childInfo in childrenToDetach)
        {
            // Recursively split the mass among all children
            DetachChildrenOnDestroy script = childInfo.child.GetComponent<DetachChildrenOnDestroy>();
            if (script)
            {
                script.SetMassToSplit(massToSplit * (childInfo.relativeMass / totalRelativeMass));

            }
        }
    }

    private void Start()
    {
        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
        if (rb2d)
        {
            SetMassToSplit(rb2d.mass);

        }

        foreach (ChildInfo childInfo in childrenToDetach)
        {
            // Ensure the obj removes itself from the list when destroyed
            ActivateOnDespawn[] despawnComponents = childInfo.child.GetComponents<ActivateOnDespawn>();
            if (despawnComponents.Length == 0) despawnComponents = new ActivateOnDespawn[] { childInfo.child.AddComponent<ActivateOnDespawn>() };
            despawnComponents[0].OnDespawned += RemoveChildFromList;

            // Ensure the obj adds itself back to the list when it was temporarily removed but not deleted
            ActivateOnSpawn[] spawnComponents = childInfo.child.GetComponents<ActivateOnSpawn>();
            if (spawnComponents.Length == 0) spawnComponents = new ActivateOnSpawn[] { childInfo.child.AddComponent<ActivateOnSpawn>() };
            spawnComponents[0].OnSpawned += AddChildToList;
        }
    }

    public void AddChildToList(GameObject obj)
    {
        foreach (var childInfo in childrenToDetach)
        {
            if (childInfo.child == obj)
            {
                return;
            }
        }
        // If the child is not already in the list, add it
       float defaultWeight = 1; // Will set it using some other logic later
        // Maybe check if obj has a component that indicates weight?
        childrenToDetach.Add(new ChildInfo() { relativeMass = defaultWeight, child = obj });
    }

    private void RemoveChildFromList(GameObject obj)
    {
        for (int i = 0; i < childrenToDetach.Count; i++)
        {
            ChildInfo childInfo = childrenToDetach[i];
            if (childInfo.child == obj)
            {
                childrenToDetach.Remove(childInfo);
                return;
            }
        }
    }

    public override void Activate()
    {
        // Run the previous implementation
        base.Activate();
        DetachChildren();
    }
    private void DetachChildren()
    {
        float totalRelativeMass = childrenToDetach.Sum(childInfo => childInfo.relativeMass);
        foreach (ChildInfo childInfo in childrenToDetach)
        {
            childInfo.child.transform.parent = null;
            Rigidbody2D rb2d = childInfo.child.AddComponent<Rigidbody2D>();
            rb2d.gravityScale = 0;
            Debug.Log("Calculating mass for child with relative mass " + childInfo.relativeMass + " out of total " + totalRelativeMass + " from parent mass " + massToSplit);
            rb2d.mass = massToSplit * (childInfo.relativeMass / totalRelativeMass);
            rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;
            CompositeCollider2D compositeCollider2D = childInfo.child.AddComponent<CompositeCollider2D>();
            compositeCollider2D.geometryType = CompositeCollider2D.GeometryType.Polygons;
            compositeCollider2D.generationType = CompositeCollider2D.GenerationType.Synchronous;
        }
    }
}
