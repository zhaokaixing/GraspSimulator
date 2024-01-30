using System;
using TriLib;
using UnityEditor;
using UnityEngine;

namespace TriLibEditor
{
    [CustomEditor(typeof(AssimpMetadataCollection))]
    public class AssimpMetadataCollectionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var assimpMetadataCollection = target as AssimpMetadataCollection;
            EditorGUILayout.BeginVertical();
            foreach (var kvp in assimpMetadataCollection)
            {
                var metadataDescription = kvp.Key;
                string metadataType;
                switch (kvp.Value.MetadataType)
                {
                    case AssimpMetadataType.AI_BOOL:
                        metadataType = "(Bool)";
                        break;
                    case AssimpMetadataType.AI_INT32:
                        metadataType = "(Int32)";
                        break;
                    case AssimpMetadataType.AI_UINT64:
                        metadataType = "(UInt64)";
                        break;
                    case AssimpMetadataType.AI_FLOAT:
                        metadataType = "(Float)";
                        break;
                    case AssimpMetadataType.AI_DOUBLE:
                        metadataType = "(Double)";
                        break;
                    case AssimpMetadataType.AI_AISTRING:
                        metadataType = "(String)";
                        break;
                    default:
                        metadataType = "(Vector3)";
                        break;
                }
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(metadataDescription, metadataType), EditorStyles.boldLabel);
                EditorGUILayout.SelectableLabel(kvp.Value.MetadataValue.ToString(), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }
    }
}
