using UnityEngine;
using EzySlice;

/**
 * An example fun script to show how a shatter operation can be applied to a GameObject
 * by repeatedly and randomly slicing an object
 */
public class ShatterExample : MonoBehaviour
{
    /**
     * This function will slice the provided object by the plane defined in this
     * GameObject. We use the GameObject this script is attached to define the position
     * and direction of our cutting Plane. Results are then returned to the user.
     */
    public SlicedHull ShatterObject(GameObject obj, int iterations, Material crossSectionMaterial = null)
    {
        while (true)
        {
            if (iterations > 0)
            {
                var plane = GetRandomPlane(obj.transform.position, obj.transform.localScale);
                var textureRegion = new TextureRegion(0.0f, 0.0f, 1.0f, 1.0f);

                SlicedHull slices = obj.SliceInstantiate(plane, textureRegion, crossSectionMaterial);

                if (slices != null)
                {
                    // shatter the shattered!
                    foreach (var slice in slices)
                    {
                        SlicedHull slidedHull = ShatterObject(slice, iterations - 1, crossSectionMaterial);
                        if (slidedHull == null)
                        {
                            // delete the parent
                            DestroyImmediate(slice); // TODO: should this be Destroy() ?
                        }
                    }

                    return slices;
                }

                --iterations;
                continue;
            }

            return null;
        }
    }

    /**
     * Given an offset position and an offset scale, calculate a random plane
     * which can be used to randomly slice an object
     */
    private EzySlice.Plane GetRandomPlane(Vector3 positionOffset, Vector3 scaleOffset)
    {
        Vector3 randomPosition = Random.insideUnitSphere;

        randomPosition += positionOffset;

        Vector3 randomDirection = Random.insideUnitSphere.normalized;

        return new EzySlice.Plane(randomPosition, randomDirection);
    }
}