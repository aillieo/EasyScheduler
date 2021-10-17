#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AillieoUtils
{
    [CustomEditor(typeof(Scheduler))]
    public class SchedulerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"{Scheduler.GetRunningInfo()}", new GUIStyle("label") { wordWrap = true });
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
