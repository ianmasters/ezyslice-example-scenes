using System;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using Random = UnityEngine.Random;

/**
 * Represents a really badly written shatter script! use for reference purposes only.
 */
public class RuntimeShatterExample : MonoBehaviour
{
    public GameObject objectToShatter;
    public Material crossSectionMaterial;

#if DEBUG
    public bool enableTestPlane;
    public GameObject testPlane;
#endif

    // TODO: make private when debugging complete so it isn't serialized
    public List<GameObject> prevShatters = new List<GameObject>();

    private BoundingSphere debugSphere;

    private SlicedHull ShatterObject(GameObject obj)
    {
        var textureRegion = new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f);
        SlicedHull slicedHull;

        EzySlice.Plane plane;
        if (enableTestPlane && testPlane)
        {
            slicedHull = obj.SliceInstantiate(
                testPlane.transform.position,
                testPlane.transform.up,
                textureRegion,
                crossSectionMaterial);

            // plane.TransformInto(obj.transform);
            // var p = testPlane.transform.position;//obj.transform.InverseTransformPoint(testPlane.transform.position);
            // var n = obj.transform.InverseTransformVector(testPlane.transform.up);
            // var m = n.magnitude;
            // // n.Normalize();
            // n = testPlane.transform.TransformVector(n);
            // m = n.magnitude;
            // n.Normalize();
            // plane.Compute(p, n);
        }
        else
        {
            var col = obj.GetComponent<Collider>();
            Debug.Assert(col);
            var objBounds = col.bounds;
            const float oneOnSqrt2 = 0.7f; // just less than 1/sqrt(2) - should produce a cut through most meshes with a tight bounds
            plane = GetRandomPlane(objBounds.center, objBounds.extents * oneOnSqrt2);
            slicedHull = obj.SliceInstantiate(
                plane,
                textureRegion,
                crossSectionMaterial);

#if DEBUG
            debugSphere = new BoundingSphere(objBounds.center, objBounds.extents.magnitude /* * oneOnSqrt2*/);
            DebugExtension.DebugWireSphere(debugSphere.position, Color.white, debugSphere.radius, 2);
#endif
        }
        
        if (slicedHull is null)
        {
            Debug.Break();
        }
        return slicedHull;
    }

    private static EzySlice.Plane GetRandomPlane(Vector3 positionOffset, Vector3 scaleOffset)
    {
        var randomPosition = Random.insideUnitSphere;

        randomPosition += positionOffset;

        var randomDirection = Random.insideUnitSphere.normalized;

        return new EzySlice.Plane(randomPosition, randomDirection);
    }

    public void Explode()
    {
        int q = 0;
    }

    private void OnValidate()
    {
        if (testPlane)
        {
            if (enableTestPlane)
            {
                testPlane.SetActive(true);
            }
            else
            {
                testPlane.SetActive(false);
            }
        }
    }

    public void Gravity()
    {
        if (prevShatters.Count > 0)
        {
            var g = !prevShatters[0].GetComponent<Rigidbody>().useGravity;
            foreach (var s in prevShatters)
            {
                s.GetComponent<Rigidbody>().useGravity = g;
            }
        }
        else
        {
            var rb = objectToShatter.GetComponent<Rigidbody>();
            rb.useGravity = !rb.useGravity;
        }
    }

    public void RandomShatter()
    {
        GameObject objectToSplit = null;

        // First shatter
        if (prevShatters.Count == 0)
        {
            objectToSplit = objectToShatter;
            // objectToShatter = null;
            print($"RandomShatter {objectToSplit.name}");
        }
        else
        {
            // otherwise, shatter the previous shattered objects, randomly picked
            do
            {
                try
                {
                    objectToSplit = prevShatters[Random.Range(0, prevShatters.Count - 1)];
                    print($"RandomShatter from {objectToSplit.name}");
                    if (!objectToSplit.activeSelf)
                    {
                        Debug.LogWarning("Tried to select an inactive shatter object");
                    }
                }
                catch (Exception)
                {
                    var q = 0;
                }
            } while (objectToSplit is null || !objectToSplit.activeSelf);
        }

        var slicedHull = ShatterObject(objectToSplit);

#if DEBUG
        if (slicedHull is null)
        {
            var q = 0;
        }
#endif

        if (slicedHull != null)
        {
            Debug.Assert(slicedHull.HullMesh(0) || slicedHull.HullMesh(1), "There should only be an upper and/or lower hull");

            var rbSource = objectToSplit.GetComponentInChildren<Rigidbody>();
            // var colSource = objectToSplit.GetComponentInChildren<Collider>();
            // var meshSource = objectToSplit.GetComponentInChildren<Mesh>();

            // add rigidbodies and colliders
            var i = 0;
            foreach (var shattered in slicedHull)
            {
                shattered.AddComponent<MeshCollider>().convex = true;
                if (rbSource)
                {
                    var rb = shattered.AddComponent<Rigidbody>();
                    rb.velocity = rbSource.velocity;
                    rb.angularVelocity = rbSource.angularVelocity;
                    rb.useGravity = rbSource.useGravity;
                    rb.isKinematic = rbSource.isKinematic;
                    rb.drag = rbSource.drag;
                    rb.angularDrag = rbSource.angularDrag;
                    var volume = slicedHull.HullVolume(i);
                    rb.mass = rbSource.mass * volume / slicedHull.sourceVolume;
                }

                prevShatters.Add(shattered);
                ++i;
            }
            // objectToSplit.SetActive(false);
            Destroy(objectToSplit);
            prevShatters.Remove(objectToSplit);
        }
    }
}