using UnityEditor;
using UnityEngine;

namespace YeKostenko.AudioEngine.Editor
{
    [CustomPropertyDrawer(typeof(SpatialSettings))]
    public sealed class SpatialSettingsDrawer : PropertyDrawer
    {
        private const float Pad = 6f;
        private const float Gap = 4f;
        private const float TitleBarHeight = 18f;

        private const float MinMaxInnerGap = 6f;
        private const float MinMaxSubLabelW = 26f; // "Min"/"Max"

        private static readonly Color Accent = new(0.20f, 0.70f, 1.00f, 1f);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty mode = property.FindPropertyRelative("_mode");
            bool is3D = mode != null && mode.enumValueIndex == (int)SpatialMode.ThreeD;

            float h = 0f;

            // header line
            h += EditorGUIUtility.singleLineHeight;

            // panel padding
            h += (Pad * 2f) + TitleBarHeight;

            // mode line
            h += Gap + EditorGUIUtility.singleLineHeight;

            if (!is3D)
            {
                // short note
                h += Gap + EditorGUIUtility.singleLineHeight;
                return h;
            }

            // 3D block: blend + min/max + rolloff + doppler + spread
            h += Gap + EditorGUIUtility.singleLineHeight; // spatialBlend
            h += Gap + EditorGUIUtility.singleLineHeight; // min/max row
            h += Gap + EditorGUIUtility.singleLineHeight; // rolloff
            h += Gap + EditorGUIUtility.singleLineHeight; // doppler
            h += Gap + EditorGUIUtility.singleLineHeight; // spread

            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty mode = property.FindPropertyRelative("_mode");
            SerializedProperty spatialBlend = property.FindPropertyRelative("_spatialBlend");
            SerializedProperty minDistance = property.FindPropertyRelative("_minDistance");
            SerializedProperty maxDistance = property.FindPropertyRelative("_maxDistance");
            SerializedProperty rolloff = property.FindPropertyRelative("_rolloff");
            SerializedProperty doppler = property.FindPropertyRelative("_dopplerLevel");
            SerializedProperty spread = property.FindPropertyRelative("_spread");

            bool is3D = mode != null && mode.enumValueIndex == (int)SpatialMode.ThreeD;

            Rect line = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // label row (keep it compact; SpatialSettings itself is drawn inline in parent inspectors)
            EditorGUI.LabelField(line, label, EditorStyles.boldLabel);

            // panel rect (below label)
            float panelY = line.yMax + Gap;
            Rect panelRect = new(position.x, panelY, position.width, position.yMax - panelY);

            DrawPanel(panelRect, is3D ? "3D Spatialization" : "2D / Disabled");

            Rect content = new(
                panelRect.x + Pad,
                panelRect.y + Pad + TitleBarHeight,
                panelRect.width - (Pad * 2f),
                panelRect.height - ((Pad * 2f) + TitleBarHeight));

            Rect r = new(content.x, content.y, content.width, EditorGUIUtility.singleLineHeight);

            // mode
            EditorGUI.PropertyField(r, mode, new GUIContent("Mode"));
            r.y += r.height + Gap;

            if (!is3D)
            {
                EditorGUI.LabelField(r, "3D fields are hidden while Mode is not ThreeD.", EditorStyles.miniLabel);
                EditorGUI.EndProperty();
                return;
            }

            // blend (slider 0..1)
            EditorGUI.Slider(r, spatialBlend, 0f, 1f, new GUIContent("Spatial Blend"));
            r.y += r.height + Gap;

            // min / max row (fixed: single label + two wide fields)
            DrawMinMaxRow(r, minDistance, maxDistance);

            // normalize if needed
            if (minDistance.floatValue < 0f)
            {
                minDistance.floatValue = 0f;
            }

            if (maxDistance.floatValue < 0f)
            {
                maxDistance.floatValue = 0f;
            }

            if (maxDistance.floatValue > 0f && minDistance.floatValue > maxDistance.floatValue)
            {
                minDistance.floatValue = maxDistance.floatValue;
            }

            r.y += r.height + Gap;

            // rolloff
            EditorGUI.PropertyField(r, rolloff, new GUIContent("Rolloff"));
            r.y += r.height + Gap;

            // doppler + spread
            EditorGUI.PropertyField(r, doppler, new GUIContent("Doppler Level"));
            r.y += r.height + Gap;

            EditorGUI.PropertyField(r, spread, new GUIContent("Spread"));

            EditorGUI.EndProperty();
        }

        private static void DrawMinMaxRow(Rect r, SerializedProperty minDistance, SerializedProperty maxDistance)
        {
            r = EditorGUI.IndentedRect(r);

            // Single label for the whole row.
            Rect labelRect = new(r.x, r.y, EditorGUIUtility.labelWidth, r.height);
            EditorGUI.LabelField(labelRect, new GUIContent("Distance"));

            // Value area for both fields.
            Rect valueRect = new(labelRect.xMax, r.y, r.xMax - labelRect.xMax, r.height);

            // Split value area into two fields.
            float half = (valueRect.width - MinMaxInnerGap) * 0.5f;
            Rect left = new(valueRect.x, r.y, half, r.height);
            Rect right = new(valueRect.x + half + MinMaxInnerGap, r.y, half, r.height);

            // Sub-label + numeric field for each side.
            Rect leftLabel = new(left.x, left.y, MinMaxSubLabelW, left.height);
            Rect leftField = new(leftLabel.xMax, left.y, left.xMax - leftLabel.xMax, left.height);

            Rect rightLabel = new(right.x, right.y, MinMaxSubLabelW, right.height);
            Rect rightField = new(rightLabel.xMax, right.y, right.xMax - rightLabel.xMax, right.height);

            EditorGUI.LabelField(leftLabel, new GUIContent("Min"), EditorStyles.miniLabel);
            EditorGUI.PropertyField(leftField, minDistance, GUIContent.none);

            EditorGUI.LabelField(rightLabel, new GUIContent("Max"), EditorStyles.miniLabel);
            EditorGUI.PropertyField(rightField, maxDistance, GUIContent.none);
        }

        private static void DrawPanel(Rect rect, string title)
        {
            // background
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.08f));

            // border
            Handles.BeginGUI();
            Color prev = Handles.color;
            Handles.color = new Color(0f, 0f, 0f, 0.25f);
            Handles.DrawAAPolyLine(
                1f,
                new Vector3(rect.x, rect.y),
                new Vector3(rect.xMax, rect.y),
                new Vector3(rect.xMax, rect.yMax),
                new Vector3(rect.x, rect.yMax),
                new Vector3(rect.x, rect.y));
            Handles.color = prev;
            Handles.EndGUI();

            // accent bar + title
            Rect bar = new(rect.x, rect.y, 3f, rect.height);
            EditorGUI.DrawRect(bar, Accent);

            Rect titleRect = new(rect.x + 8f, rect.y + 2f, rect.width - 8f, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(titleRect, title, EditorStyles.miniBoldLabel);
        }
    }
}