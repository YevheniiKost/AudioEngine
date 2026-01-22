using UnityEditor;
using UnityEngine;

namespace YeKostenko.AudioEngine.Editor
{
    [CustomEditor(typeof(MusicTrack))]
    public sealed class MusicTrackEditor : UnityEditor.Editor
    {
        private SerializedProperty _main;
        private SerializedProperty _route;
        private SerializedProperty _baseVolume;
        private SerializedProperty _loop;
        private SerializedProperty _fade;
        private SerializedProperty _priority;
        private SerializedProperty _debugName;

        private bool _foldClips = true;
        private bool _foldMixing = true;
        private bool _foldPlayback = true;
        private bool _foldPriority = true;
        private bool _foldMeta = true;

        private void OnEnable()
        {
            _main = serializedObject.FindProperty("_main");
            _route = serializedObject.FindProperty("_route");
            _baseVolume = serializedObject.FindProperty("_baseVolume");
            _loop = serializedObject.FindProperty("_loop");
            _fade = serializedObject.FindProperty("_fade");
            _priority = serializedObject.FindProperty("_priority");
            _debugName = serializedObject.FindProperty("_debugName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();

            EditorGUILayout.Space(8);

            DrawClips();
            EditorGUILayout.Space(6);

            DrawMixing();
            EditorGUILayout.Space(6);

            DrawPlayback();
            EditorGUILayout.Space(6);

            DrawPriority();
            EditorGUILayout.Space(6);

            DrawMeta();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUIStyle titleStyle = new(EditorStyles.boldLabel)
                    {
                        fontSize = 12
                    };

                    EditorGUILayout.LabelField("Music Track", titleStyle);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Ping", GUILayout.Width(54)))
                    {
                        EditorGUIUtility.PingObject(target);
                    }
                }

                string mainName = GetMainClipNameOrStatus();
                EditorGUILayout.LabelField($"Main: {mainName}", EditorStyles.miniLabel);
            }
        }

        private string GetMainClipNameOrStatus()
        {
            if (_main == null)
            {
                return "Unknown";
            }

            if (_main.propertyType == SerializedPropertyType.ObjectReference)
            {
                return _main.objectReferenceValue != null ? _main.objectReferenceValue.name : "None";
            }

            if (_main.isArray)
            {
                return $"Array ({_main.arraySize})";
            }

            return _main.displayName;
        }

        private void DrawClips()
        {
            _foldClips = DrawFoldout(_foldClips, "Clips");
            if (!_foldClips)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_main, new GUIContent("Main"), true);

                if (_main != null && _main.propertyType == SerializedPropertyType.ObjectReference &&
                    _main.objectReferenceValue == null)
                {
                    DrawInfoBox("No main clip assigned. This track will not play anything.", MessageType.Warning);
                }
            }
        }

        private void DrawMixing()
        {
            _foldMixing = DrawFoldout(_foldMixing, "Mixing");
            if (!_foldMixing)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_route, new GUIContent("Mixer Route"));

                using (new EditorGUI.IndentLevelScope())
                {
                    if (_baseVolume != null)
                    {
                        _baseVolume.floatValue = Mathf.Clamp01(_baseVolume.floatValue);
                    }

                    EditorGUILayout.Slider(_baseVolume, 0f, 1f, new GUIContent("Base Volume"));
                }
            }
        }

        private void DrawPlayback()
        {
            _foldPlayback = DrawFoldout(_foldPlayback, "Playback");
            if (!_foldPlayback)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_loop, new GUIContent("Loop"));

                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(_fade, new GUIContent("Fade Settings"), true);
                }
            }
        }

        private void DrawPriority()
        {
            _foldPriority = DrawFoldout(_foldPriority, "Priority");
            if (!_foldPriority)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_priority, new GUIContent("Priority Settings"), true);
            }
        }

        private void DrawMeta()
        {
            _foldMeta = DrawFoldout(_foldMeta, "Meta");
            if (!_foldMeta)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_debugName, new GUIContent("Debug Name"));
            }
        }

        private static bool DrawFoldout(bool value, string title)
        {
            GUIStyle style = new(EditorStyles.foldoutHeader)
            {
                fontStyle = FontStyle.Bold
            };

            return EditorGUILayout.BeginFoldoutHeaderGroup(value, title, style).AlsoEnd();
        }

        private static void DrawInfoBox(string message, MessageType type) => EditorGUILayout.HelpBox(message, type);
    }
}