using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace YellowTape.AudioEngine
{
    [CustomEditor(typeof(SoundComponent))]
    public class SoundComponentEditor : Editor
    {
        private AudioEngineConfig _config;
        private string[] _allClips;
        private int _chosenClipIndex;
        
        public override void OnInspectorGUI()
        {
            SoundComponent soundComponent = (SoundComponent)target;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Sound Type");
            ApplySoundType(soundComponent);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            ApplyClip(soundComponent);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Connect To Button"))
            {
                soundComponent.AutoConnectToButtonComponent();
                EditorUtility.SetDirty(soundComponent);
                Undo.RecordObject(soundComponent, "Auto Connect To Button");
                PrefabUtility.RecordPrefabInstancePropertyModifications(soundComponent);
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private static void ApplySoundType(SoundComponent soundComponent)
        {
            SoundType newSoundType = (SoundType)EditorGUILayout.EnumPopup(soundComponent.SoundType);
            if (newSoundType != soundComponent.SoundType)
            {
                EditorUtility.SetDirty(soundComponent);
                Undo.RecordObject(soundComponent, "Change Sound Type");
                PrefabUtility.RecordPrefabInstancePropertyModifications(soundComponent);
            }

            soundComponent.SoundType = newSoundType;
        }

        public void OnEnable()
        {
            _config = AudioEngineHelper.LoadConfig();
            SoundComponent soundComponent = (SoundComponent)target;
            if (!string.IsNullOrEmpty(soundComponent.Name))
            {
                _chosenClipIndex = Array.IndexOf(GetAllClipsName(soundComponent.SoundType), soundComponent.Name);
            }
        }

        private string[] GetAllClipsName(SoundType type)
        {
            var parameter = _config.Parameters.Find(t => t.SoundType == type);
            return parameter.AllClips.Select(x => x.name).ToArray();
        }
        
        private void ApplyClip(SoundComponent soundComponent)
        {
            _allClips = GetAllClipsName(soundComponent.SoundType);
            if (_allClips.Length > 0)
            {
                EditorGUILayout.LabelField("Name");
                _chosenClipIndex = EditorGUILayout.Popup(_chosenClipIndex, _allClips);
                string newClipName = _allClips[_chosenClipIndex];
                
                if (string.IsNullOrEmpty(newClipName))
                    return;
                
                if (!soundComponent.Name.Equals(newClipName))
                {
                    EditorUtility.SetDirty(soundComponent);
                    Undo.RecordObject(soundComponent, "Change Clip Name");
                    PrefabUtility.RecordPrefabInstancePropertyModifications(soundComponent);
                }

                soundComponent.Name = _allClips[_chosenClipIndex];
            }
            else
            {
                EditorGUILayout.LabelField("No clips found");
            }
        }
    }
}