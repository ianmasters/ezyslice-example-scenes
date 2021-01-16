using UnityEngine;
using EzySlice;
using Random = UnityEngine.Random;

/**
 * An example fun script to show how a shatter operation can be applied to a GameObject
 * by repeatedly and randomly slicing an object
 */
public class ShatterExample : MonoBehaviour
{
    public GameObject objectToShatter;
    public Material crossSectionMaterial;
    public int shatterCount;

    /**
     * This function will shatterCount the provided object by the plane defined by this
     * GameObject. We use the GameObject this script is attached to define the position
     * and direction of our cutting Plane. Results are then returned to the user.
     */
    public SlicedHull ShatterObject(GameObject obj, int iterations, Material crossSectionMat)
    {
        Debug.Assert(iterations >= 1);
        
        // var plane = GetRandomPlane(obj.transform.position, obj.transform.localScale);
        var plane = new EzySlice.Plane();
        plane.Compute(gameObject);
        plane.TransformInto(obj.transform);

        var textureRegion = new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f);

        SlicedHull slices = obj.SliceInstantiate(plane, textureRegion, crossSectionMat);

        while (iterations > 0)
        {
            if (!(slices is null))
            {
                // shatter the shattered!
                foreach (var slice in slices)
                {
                    SlicedHull slidedHull = ShatterObject(slice, iterations - 1, crossSectionMat);
                    if (slidedHull == null)
                    {
                        // delete the parent
                        DestroyImmediate(slice);
                    }
                }

                return slices;
            }
            --iterations;
        }
        return null;
    }

    // /**
    //  * Given an offset position and an offset scale, calculate a random plane
    //  * which can be used to randomly shatterCount an object
    //  */
    // private EzySlice.Plane GetRandomPlane(in Vector3 positionOffset, in Vector3 scaleOffset)
    // {
    //     var randomPosition = Random.insideUnitSphere;
    //     // randomPosition.Scale(scaleOffset);
    //
    //     randomPosition += positionOffset;
    //
    //     var randomDirection = Random.insideUnitSphere.normalized;
    //
    //     return new EzySlice.Plane(randomPosition, randomDirection);
    // }
}