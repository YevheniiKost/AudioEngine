using UnityEditor;

using UnityEngine;

namespace YeKostenko.AudioEngine.Editor
{
    [CustomEditor(typeof(AudioEvent))]
    public sealed class AudioEventEditor : UnityEditor.Editor
    {
        private const float PitchSliderMin = 0.25f;
        private const float PitchSliderMax = 2.0f;
        private SerializedProperty _clips;
        private SerializedProperty _cooldown;

        private SerializedProperty _debugName;
        private SerializedProperty _fadeSettings;

        private bool _foldClips = true;
        private bool _foldMeta;
        private bool _foldMixing = true;
        private bool _foldPlayback = true;
        private bool _foldPriority = true;
        private bool _foldRandom = true;
        private bool _foldSpatial = true;

        private SerializedProperty _loop;
        private SerializedProperty _maxVoices;
        private SerializedProperty _mixerRoute;
        private SerializedProperty _pitchMax;
        private SerializedProperty _pitchMin;
        private SerializedProperty _prioritySettings;
        private SerializedProperty _soundBus;

        private SerializedProperty _spatialSettings;
        private SerializedProperty _volumeMax;

        private SerializedProperty _volumeMin;

        private void OnEnable()
        {
            _clips = serializedObject.FindProperty("_clips");

            _soundBus = serializedObject.FindProperty("_soundBus");
            _mixerRoute = serializedObject.FindProperty("_mixerRoute");
            _cooldown = serializedObject.FindProperty("_cooldown");

            _volumeMin = serializedObject.FindProperty("_volumeMin");
            _volumeMax = serializedObject.FindProperty("_volumeMax");
            _pitchMin = serializedObject.FindProperty("_pitchMin");
            _pitchMax = serializedObject.FindProperty("_pitchMax");

            _loop = serializedObject.FindProperty("_loop");
            _maxVoices = serializedObject.FindProperty("_maxVoices");
            _fadeSettings = serializedObject.FindProperty("_fadeSettings");

            _spatialSettings = serializedObject.FindProperty("_spatialSettings");
            _prioritySettings = serializedObject.FindProperty("_prioritySettings");

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

            DrawRandomization();
            EditorGUILayout.Space(6);

            DrawPlayback();
            EditorGUILayout.Space(6);

            DrawSpatial();
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

                    EditorGUILayout.LabelField("Audio Event", titleStyle);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Ping", GUILayout.Width(54)))
                    {
                        EditorGUIUtility.PingObject(target);
                    }
                }

                int clipCount = _clips != null ? _clips.arraySize : 0;
                EditorGUILayout.LabelField($"Clips: {clipCount}", EditorStyles.miniLabel);
                SerializedProperty spatialMode = _spatialSettings.FindPropertyRelative("_mode");
                string spatialModeName =
                    spatialMode != null ? ((SpatialMode)spatialMode.enumValueIndex).ToString() : "Unknown";
                EditorGUILayout.LabelField($"Spatial Mode: {spatialModeName}", EditorStyles.miniLabel);
            }
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
                EditorGUILayout.PropertyField(_clips, new GUIContent("Audio Clips"), true);

                if (_clips.arraySize == 0)
                {
                    DrawInfoBox("No clips assigned. This event will not play anything.", MessageType.Warning);
                }
                else
                {
                    bool hasNull = false;
                    for (int i = 0; i < _clips.arraySize; i++)
                    {
                        SerializedProperty element = _clips.GetArrayElementAtIndex(i);
                        if (element.objectReferenceValue == null)
                        {
                            hasNull = true;
                            break;
                        }
                    }

                    if (hasNull)
                    {
                        DrawInfoBox("Some clip slots are empty (null).", MessageType.Warning);
                    }
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
                EditorGUILayout.PropertyField(_soundBus, new GUIContent("Sound Bus"));
                EditorGUILayout.PropertyField(_mixerRoute, new GUIContent("Mixer Route"));

                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(_cooldown, new GUIContent("Cooldown (s)"));
                }

                if (_cooldown.floatValue < 0f)
                {
                    _cooldown.floatValue = 0f;
                }
            }
        }

        private void DrawRandomization()
        {
            _foldRandom = DrawFoldout(_foldRandom, "Randomization");
            if (!_foldRandom)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawMinMaxSlider01("Volume (0..1)", _volumeMin, _volumeMax);

                EditorGUILayout.Space(4);

                DrawMinMaxSlider("Pitch", _pitchMin, _pitchMax, PitchSliderMin, PitchSliderMax);

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Reset", GUILayout.Width(72)))
                    {
                        _volumeMin.floatValue = 1f;
                        _volumeMax.floatValue = 1f;
                        _pitchMin.floatValue = 1f;
                        _pitchMax.floatValue = 1f;
                    }
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
                    EditorGUILayout.PropertyField(_maxVoices, new GUIContent("Max Voices"));
                    if (_maxVoices.intValue < 0)
                    {
                        _maxVoices.intValue = 0;
                    }

                    EditorGUILayout.PropertyField(_fadeSettings, new GUIContent("Fade Settings"));
                }

                if (_loop.boolValue && _maxVoices.intValue == 0)
                {
                    DrawInfoBox("Loop is enabled but Max Voices is 0. Nothing will play.", MessageType.Warning);
                }
            }
        }

        private void DrawSpatial()
        {
            _foldSpatial = DrawFoldout(_foldSpatial, "Spatial");
            if (!_foldSpatial)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_spatialSettings, new GUIContent("Spatial Settings"), true);
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
                EditorGUILayout.PropertyField(_prioritySettings, new GUIContent("Priority Settings"), true);
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

#if UNITY_2020_1_OR_NEWER
            return EditorGUILayout.BeginFoldoutHeaderGroup(value, title, style).AlsoEnd();
#else
            return EditorGUILayout.Foldout(value, title, true, style);
#endif
        }

        private static void DrawMinMaxSlider01(string label, SerializedProperty minProp, SerializedProperty maxProp)
        {
            minProp.floatValue = Mathf.Clamp01(minProp.floatValue);
            maxProp.floatValue = Mathf.Clamp01(maxProp.floatValue);
            if (minProp.floatValue > maxProp.floatValue)
            {
                minProp.floatValue = maxProp.floatValue;
            }

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

                float min = minProp.floatValue;
                float max = maxProp.floatValue;

                EditorGUILayout.MinMaxSlider(ref min, ref max, 0f, 1f);

                using (new EditorGUILayout.HorizontalScope())
                {
                    min = EditorGUILayout.FloatField(new GUIContent("Min"), min, GUILayout.MinWidth(60));
                    max = EditorGUILayout.FloatField(new GUIContent("Max"), max, GUILayout.MinWidth(60));
                }

                minProp.floatValue = Mathf.Clamp01(min);
                maxProp.floatValue = Mathf.Clamp01(max);
                if (minProp.floatValue > maxProp.floatValue)
                {
                    minProp.floatValue = maxProp.floatValue;
                }
            }
        }

        private static void DrawMinMaxSlider(string label, SerializedProperty minProp, SerializedProperty maxProp,
            float minLimit, float maxLimit)
        {
            if (minLimit > maxLimit)
            {
                float tmp = minLimit;
                minLimit = maxLimit;
                maxLimit = tmp;
            }

            minProp.floatValue = Mathf.Clamp(minProp.floatValue, minLimit, maxLimit);
            maxProp.floatValue = Mathf.Clamp(maxProp.floatValue, minLimit, maxLimit);
            if (minProp.floatValue > maxProp.floatValue)
            {
                minProp.floatValue = maxProp.floatValue;
            }

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

                float min = minProp.floatValue;
                float max = maxProp.floatValue;

                EditorGUILayout.MinMaxSlider(ref min, ref max, minLimit, maxLimit);

                using (new EditorGUILayout.HorizontalScope())
                {
                    min = EditorGUILayout.FloatField(new GUIContent("Min"), min, GUILayout.MinWidth(60));
                    max = EditorGUILayout.FloatField(new GUIContent("Max"), max, GUILayout.MinWidth(60));
                }

                minProp.floatValue = Mathf.Clamp(min, minLimit, maxLimit);
                maxProp.floatValue = Mathf.Clamp(max, minLimit, maxLimit);
                if (minProp.floatValue > maxProp.floatValue)
                {
                    minProp.floatValue = maxProp.floatValue;
                }
            }
        }

        private static void DrawInfoBox(string message, MessageType type) => EditorGUILayout.HelpBox(message, type);
    }

#if UNITY_2020_1_OR_NEWER
    internal static class FoldoutHeaderGroupExtensions
    {
        public static bool AlsoEnd(this bool value)
        {
            EditorGUILayout.EndFoldoutHeaderGroup();
            return value;
        }
    }
#endif
}