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

    [SerializeField] private Rigidbody2D topRigidbody2D; // Make private after testing
    [SerializeField] private List<ChildInfo> childrenToDetach;
    [SerializeField] private DetachChildrenOnDestroy parentScript; // Make private after testing
    [SerializeField] private float totalMass; // Make private after testing
    [SerializeField] private float myRelativeMass = 1f; // Small fraction for itself

    public void SetParentScript(DetachChildrenOnDestroy parent)
    {
        parentScript = parent;
    }

    public void SetMassToSplit(float mass)
    {
        totalMass = mass;
        float totalRelativeMass = childrenToDetach.Sum(childInfo => childInfo.relativeMass) + myRelativeMass;
        foreach (ChildInfo childInfo in childrenToDetach)
        {
            DetachChildrenOnDestroy script = childInfo.child.GetComponent<DetachChildrenOnDestroy>();
            if (script)
            {
                script.SetParentScript(this);
                script.SetMassToSplit(totalMass * (childInfo.relativeMass / totalRelativeMass));
            }
        }
    }

    private void Start()
    {
        if (topRigidbody2D)
        {
            totalMass = topRigidbody2D.mass;
        }
        SetMassToSplit(totalMass);

        // Subscribe to despawn and spawn events of each child
        foreach (ChildInfo childInfo in childrenToDetach)
        {
            SubscribeToChild(childInfo.child);
        }
    }

    private void SubscribeToChild(GameObject child)
    {
        ActivateOnDespawn[] despawnComponents = child.GetComponents<ActivateOnDespawn>();
        if (despawnComponents.Length == 0)
        {
            despawnComponents = new ActivateOnDespawn[] { child.AddComponent<ActivateOnDespawn>() };
        }
        despawnComponents[0].OnDespawned += RemoveChildFromList;

        //ActivateOnSpawn[] spawnComponents = child.GetComponents<ActivateOnSpawn>();
        //if (spawnComponents.Length == 0)
        //{
        //    spawnComponents = new ActivateOnSpawn[] { child.AddComponent<ActivateOnSpawn>() };
        //}
        //spawnComponents[0].OnSpawned += AddChildToList;
    }

    // Call this to add a child dynamically at runtime
    public void AddChildToList(GameObject obj)
    {
        if (childrenToDetach.Any(c => c.child == obj)) return;

        float relativeMass = 1f;
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb)
        {
            relativeMass = rb.mass;
            totalMass += rb.mass;
            Destroy(rb);
        }
        CompositeCollider2D comp = obj.GetComponent<CompositeCollider2D>();
        if (comp) Destroy(comp);
        Collider2D col = obj.GetComponent<Collider2D>();
        if (col) col.usedByComposite = false;

        childrenToDetach.Add(new ChildInfo { relativeMass = relativeMass, child = obj });
        SetMassToSplit(totalMass);
        SubscribeToChild(obj);
    }

    private void RemoveChildFromList(GameObject obj)
    {
        ChildInfo toRemove = childrenToDetach.Find(c => c.child == obj);
        if (toRemove.child != null)
        {
            float oldTotalRelative = childrenToDetach.Sum(c => c.relativeMass) + myRelativeMass;
            float removedPortion = toRemove.relativeMass / oldTotalRelative;
            totalMass -= totalMass * removedPortion;
            childrenToDetach.Remove(toRemove);
            SetMassToSplit(totalMass);

            if (parentScript)
            {
                parentScript.RemoveChildMass(toRemove.relativeMass);
            }
        }
    }

    public void RemoveChildMass(float childRelativeMass)
    {
        float totalRelativeMass = childrenToDetach.Sum(c => c.relativeMass) + myRelativeMass;
        float removedPortion = childRelativeMass / totalRelativeMass;
        totalMass -= totalMass * removedPortion;
        SetMassToSplit(totalMass);

        if (parentScript)
        {
            parentScript.RemoveChildMass(myRelativeMass);
        }
    }

    public override void Activate()
    {
        base.Activate();
        DetachChildren();
        // When I despawn, my mass will be removed from parent
    }

    private void DetachChildren()
    {
        float totalRelativeMass = childrenToDetach.Sum(childInfo => childInfo.relativeMass) + myRelativeMass;

        foreach (ChildInfo childInfo in childrenToDetach)
        {
            childInfo.child.transform.parent = null;
            Rigidbody2D rb2d = childInfo.child.GetComponent<Rigidbody2D>();
            if (!rb2d) rb2d = childInfo.child.AddComponent<Rigidbody2D>();
            rb2d.mass = totalMass * (childInfo.relativeMass / totalRelativeMass);
            rb2d.gravityScale = 0;
            rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;

            CompositeCollider2D compositeCollider2D = childInfo.child.GetComponent<CompositeCollider2D>();
            if (!compositeCollider2D) compositeCollider2D = childInfo.child.AddComponent<CompositeCollider2D>();
            compositeCollider2D.geometryType = CompositeCollider2D.GeometryType.Polygons;
            compositeCollider2D.generationType = CompositeCollider2D.GenerationType.Synchronous;

            Collider2D collider2D = childInfo.child.GetComponent<Collider2D>();
            if (collider2D) collider2D.usedByComposite = true;
        }

        // Clear the list after detaching
        childrenToDetach.Clear();
    }
}
