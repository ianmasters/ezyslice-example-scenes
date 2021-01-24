using System;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using Random = UnityEngine.Random;

/**
 * Represents a really badly written shatter script! use for reference purposes only.
 */
// ReSharper disable once CheckNamespace
public class Shatter : MonoBehaviour
{
    public GameObject objectToShatter;
    public Material crossSectionMaterial;
    public int shatterCount;

#if DEBUG
    public bool enableTestPlane;
    public GameObject testPlane;
#endif

    private void Awake()
    {
        gameObject.SetActive(true);
    }

    public List<GameObject> prevShatters = new List<GameObject>();

    private BoundingSphere debugSphere;

    private SlicedHull ShatterObject(GameObject obj)
    {
        var textureRegion = new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f);
        SlicedHull slicedHull;

        if (enableTestPlane && testPlane)
        {
            var plane = new EzySlice.Plane(testPlane.transform.position, testPlane.transform.up);
            plane.TransformInto(obj.transform);
            slicedHull = obj.SliceInstantiate(
                plane,
                textureRegion,
                crossSectionMaterial);
        }
        else
        {
            var col = obj.GetComponent<Collider>();
            Debug.Assert(col);
            var objBounds = col.bounds;
            const float oneOnSqrt2 = 0.7f; // just less than 1/sqrt(2) - should produce a cut through most meshes with a tight bounds
            var plane = GetRandomPlane(objBounds.center, objBounds.extents * oneOnSqrt2);
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

    private static EzySlice.Plane GetRandomPlane(in Vector3 positionOffset, in Vector3 scale)
    {
        var randomPosition = Random.insideUnitSphere;

        randomPosition += positionOffset;

        var randomDirection = Random.insideUnitSphere.normalized;

        return new EzySlice.Plane(randomPosition, randomDirection, scale);
    }

    public void Explode()
    {
        // var q = 0;
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        if (testPlane)
        {
            testPlane.SetActive(enableTestPlane);
        }
    }
#endif

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
        prevShatters = new List<GameObject>();

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
                        Debug.LogWarning("Tried to select an inactive shatter object", objectToSplit);
                    }
                }
                catch (Exception)
                {
                    // var q = 0;
                }
            } while (objectToSplit is null || !objectToSplit.activeSelf);
        }

        var slicedHull = ShatterObject(objectToSplit);

#if DEBUG
        if (slicedHull is null)
        {
            // var q = 0;
        }
#endif

        if (slicedHull != null)
        {
            Debug.Assert(slicedHull.HullMesh(0) || slicedHull.HullMesh(1), "There should only be an upper and/or lower hull");

            // add rigidbodies and colliders
            var rbSource = objectToSplit.GetComponentInChildren<Rigidbody>();
            for (var i = 0; i < 2; ++i)
            {
                var shattered = slicedHull.HullObject(i);
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
                    rb.mass = rbSource.mass * slicedHull.HullVolume(i) / slicedHull.SourceVolume;
                }

                prevShatters.Add(shattered);
            }

            if (Application.isEditor)
                DestroyImmediate(objectToSplit);
            else
                Destroy(objectToSplit);

            prevShatters.Remove(objectToSplit);
        }
    }
}