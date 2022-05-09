using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapDisplay))]
public class MapDisplayEditor : Editor
{
   public override void OnInspectorGUI(){
       MapDisplay display = (MapDisplay)target;

        if(DrawDefaultInspector()){ 
            if (display.autoUpdate){
                display.DrawMapInEditor();
            }
        }

       if (GUILayout.Button ("Generate"))
       {
           display.DrawMapInEditor();
       }
       
   }
}