
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Shift))]
public class ShiftEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Shift shift = (Shift)target;

        if (GUILayout.Button("Regenerate Goals"))
        {
            shift.GenerateGoals();
        }

    }
}