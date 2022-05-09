using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Updateable), true)]
public class UpdateableEditor : Editor
{
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        Updateable updateable = (Updateable) target;

        if(GUILayout.Button("Update")){
            updateable.NotifyOfUpdatedValues();
            EditorUtility.SetDirty(target);
        }
    }
}