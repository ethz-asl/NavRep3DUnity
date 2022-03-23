using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(skinnedCollider))]
public class skinnedColliderEditor : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        skinnedCollider myTarget = (skinnedCollider)target;

        if(GUILayout.Button("Create basic colliders"))
        {
            myTarget.SetupBasicColliders();
        }

        if(GUILayout.Button("Remove basic colliders"))
        {
            myTarget.RemoveBasicColliders();
        }
    }
}