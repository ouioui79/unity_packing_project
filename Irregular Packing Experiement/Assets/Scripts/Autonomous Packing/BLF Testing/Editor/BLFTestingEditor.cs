using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BLFTesting))]
public class BLFTestingEditor : Editor
{
    string[] _choices = new string[] { "One Cube", "Random Objects", "All Objects", "All Objects Randomly", "Control With Cubes" };
    int _choiceIndex;

    public override void OnInspectorGUI()
    {
        BLFTesting myTester = (BLFTesting)target;

        DrawDefaultInspector();

        _choiceIndex = EditorGUILayout.Popup("Mode", _choiceIndex, _choices);
        //Update the selected choice in the script
        myTester.mode = _choices[_choiceIndex];
    }
}
