using UnityEngine;
using UnityEditor;

/**
 * This is a simple Editor helper script for rapid testing/prototyping! 
 */
[CustomEditor(typeof(Shatter))]
// ReSharper disable once CheckNamespace
public class ShatterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Shatter script = (Shatter)target;

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

        script.enableTestPlane = EditorGUILayout.Toggle("Enable Test Plane", script.enableTestPlane);
        if (script.enableTestPlane)
            script.testPlane = (GameObject)EditorGUILayout.ObjectField("Test Plane", script.testPlane, typeof(GameObject), true);
        else
            script.shatterCount = EditorGUILayout.IntSlider("Shatter Count", script.shatterCount, 1, 20);

        if (GUILayout.Button($"\n{(script.enableTestPlane ? "Slice" : "Shatter")} {script.objectToShatter.name}\n"))
        {
            const string undoName = "Shatter";

            // Don't think this is required as the mouse down or some other event should have incremented it.
            // This stuff is extremely unclear from any of the documentation as to when you should need to create an undo group.
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(undoName);

            Undo.RegisterFullObjectHierarchyUndo(script.objectToShatter, undoName);

            // Perform the action
            for (var i = 0; i < script.shatterCount; ++i)
            {
                script.RandomShatter();
            }

            foreach (var o in script.prevShatters)
            {
                Undo.RegisterCreatedObjectUndo(o, undoName);
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        if (GUI.changed) // once you have an editor script apparently you are responsible for doing this
        {
            script.OnValidate();
        }
    }
}