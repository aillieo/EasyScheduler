#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AillieoUtils
{
    [CustomEditor(typeof(SchedulerImpl))]
    public class SchedulerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Unity Time Scale:");
            EditorGUILayout.BeginHorizontal();
            float timeScale = Time.timeScale;
            timeScale = EditorGUILayout.Slider(timeScale, 0, 16);
            Time.timeScale = timeScale;

            if (GUILayout.Button("Reset"))
            {
                Time.timeScale = 1;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Scheduler Time Scale:");
            EditorGUILayout.BeginHorizontal();
            float globalTimeScale = Scheduler.GlobalTimeScale;
            globalTimeScale = EditorGUILayout.Slider(globalTimeScale, 0, 16);
            Scheduler.GlobalTimeScale = globalTimeScale;

            if (GUILayout.Button("Reset"))
            {
                Scheduler.GlobalTimeScale = 1;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"{Scheduler.GetRunningInfo()}", new GUIStyle("label") { wordWrap = true });
            EditorGUILayout.EndVertical();
        }
    }
}
#endif
