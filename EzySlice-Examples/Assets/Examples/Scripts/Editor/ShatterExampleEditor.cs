using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using EzySlice;

/**
 * This is a simple Editor helper script for rapid testing/prototyping! 
 */
[CustomEditor(typeof(ShatterExample))]
public class ShatterExampleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ShatterExample script = (ShatterExample)target;

        script.objectToShatter = (GameObject)EditorGUILayout.ObjectField("Object to Shatter", script.objectToShatter, typeof(GameObject), true);
        
        if (!script.objectToShatter)
        {
            EditorGUILayout.LabelField("Add a GameObject to Shatter.");
            return;
        }

        if (!script.objectToShatter.activeInHierarchy)
        {
            EditorGUILayout.LabelField("Object to slice is Hidden. Cannot Slice.");
            return;
        }
    
        script.crossSectionMaterial = (Material)EditorGUILayout.ObjectField("Cross Section Material", script.crossSectionMaterial, typeof(Material), false);

        script.shatterCount = EditorGUILayout.IntSlider("Shatter Count", script.shatterCount, 1, 20);

        if (GUILayout.Button("Shatter Object"))
        {
            // Undo.IncrementCurrentGroup();
            // Undo.SetCurrentGroupName("Shatter");
            var objects = new List<Object>();
            objects.Add(script.objectToShatter);
            // objects.Add(script.gameObject);
            var hull = script.ShatterObject(script.objectToShatter, script.shatterCount, script.crossSectionMaterial);
            if (hull != null)
            {
                foreach (var o in hull)
                    objects.Add(o);
                script.objectToShatter.SetActive(false);
            }
            Undo.RegisterCompleteObjectUndo(objects.ToArray(), "Shatter");
        }
    }
}