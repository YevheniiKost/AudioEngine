using System.Linq;
using UnityEditor;
using UnityEngine;

namespace YellowTape.AudioEngine
{
    [CustomEditor(typeof(MultiSoundComponent))]
    public class MultiSoundComponentEditor : Editor
    {
        private AudioEngineConfig _config;
        
        public override void OnInspectorGUI()
        {
            MultiSoundComponent multiSoundComponent = (MultiSoundComponent)target;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("All Sounds");
            if(GUILayout.Button("Add"))
            {
                multiSoundComponent.SoundComponentEntities.Add(new SoundComponentEntity());
            }
            EditorGUILayout.EndHorizontal();

            DrawComponents(multiSoundComponent);
        }
        
        public void OnEnable()
        {
            _config = AudioEngineHelper.LoadConfig();
        }

        private void DrawComponents(MultiSoundComponent multiSoundComponent)
        {
            if (multiSoundComponent.SoundComponentEntities == null)
                return;
            if(multiSoundComponent.SoundComponentEntities.Count == 0)
                return;

            for (int i = 0; i < multiSoundComponent.SoundComponentEntities.Count; i++)
            {
                SoundComponentEntity entity = multiSoundComponent.SoundComponentEntities[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Index");
                EditorGUILayout.LabelField($"{i}");
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Sound Type");
                ApplySoundType(entity, multiSoundComponent);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                ApplyClip(entity, multiSoundComponent);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(20);
                if(GUILayout.Button("Remove"))
                {
                    multiSoundComponent.SoundComponentEntities.RemoveAt(i);
                    return;
                }
                EditorGUILayout.Space(20);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
        }

        private void ApplySoundType(SoundComponentEntity entity, MultiSoundComponent component)
        {
            SoundType newSoundType = (SoundType)EditorGUILayout.EnumPopup(entity.SoundType);
            if (newSoundType != entity.SoundType)
            {
                EditorUtility.SetDirty(component);
                Undo.RecordObject(component, "Change Sound Type");
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            }

            entity.SoundType = newSoundType;
        }
        
        private void ApplyClip(SoundComponentEntity entity, MultiSoundComponent component)
        {
            string[] allClipsName = GetAllClipsName(entity.SoundType);
            if (allClipsName.Length > 0)
            {
                EditorGUILayout.LabelField("Name");
                int chosenClipIndex = 0;
                for (var i = 0; i < allClipsName.Length; i++)
                {
                    if (allClipsName[i].Equals(entity.Name))
                    {
                        chosenClipIndex = i;
                        break;
                    }
                }
                chosenClipIndex = EditorGUILayout.Popup(chosenClipIndex, allClipsName);
                string newClipName = allClipsName[chosenClipIndex];
                if (string.IsNullOrEmpty(entity.Name) || !entity.Name.Equals(newClipName))
                {
                    EditorUtility.SetDirty(component);
                    Undo.RecordObject(component, "Change Clip Name");
                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                }

                entity.Name = allClipsName[chosenClipIndex];
            }
            else
            {
                EditorGUILayout.LabelField("No clips found");
            }
        }
        
        private string[] GetAllClipsName(SoundType type)
        {
            var parameter = _config.Parameters.Find(t => t.SoundType == type);
            return parameter.AllClips.Select(x => x.name).ToArray();
        }
    }
}