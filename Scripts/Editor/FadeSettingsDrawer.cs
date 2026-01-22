using UnityEditor;

using UnityEngine;

namespace YeKostenko.AudioEngine.Editor
{
    [CustomPropertyDrawer(typeof(FadeSettings))]
    public sealed class FadeSettingsDrawer : PropertyDrawer
    {
        private const float Pad = 6f;
        private const float Gap = 4f;
        private const float TitleBarHeight = 18f;

        private static readonly Color Accent = new(1.00f, 0.55f, 0.15f, 1f);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty mode = property.FindPropertyRelative("_mode");
            SerializedProperty fadeIn = property.FindPropertyRelative("_in");
            SerializedProperty fadeOut = property.FindPropertyRelative("_out");

            bool hasIn = HasIn(mode);
            bool hasOut = HasOut(mode);

            float h = 0f;

            // header line
            h += EditorGUIUtility.singleLineHeight;

            // panel padding + title bar
            h += (Pad * 2f) + TitleBarHeight;

            // mode line
            h += Gap + EditorGUIUtility.singleLineHeight;

            if (hasIn)
            {
                h += Gap + EditorGUIUtility.singleLineHeight;
            }

            if (hasOut)
            {
                h += Gap + EditorGUIUtility.singleLineHeight;
            }

            if (!hasIn && !hasOut)
            {
                h += Gap + EditorGUIUtility.singleLineHeight;
            }

            _ = fadeIn;
            _ = fadeOut;

            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty mode = property.FindPropertyRelative("_mode");
            SerializedProperty fadeIn = property.FindPropertyRelative("_in");
            SerializedProperty fadeOut = property.FindPropertyRelative("_out");

            bool hasIn = HasIn(mode);
            bool hasOut = HasOut(mode);

            Rect line = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(line, label, EditorStyles.boldLabel);

            float panelY = line.yMax + Gap;
            Rect panelRect = new(position.x, panelY, position.width, position.yMax - panelY);

            DrawPanel(panelRect, hasIn || hasOut ? "Fade" : "Fade (Off)");

            // content starts below title bar
            Rect content = new(
                panelRect.x + Pad,
                panelRect.y + Pad + TitleBarHeight,
                panelRect.width - (Pad * 2f),
                panelRect.height - ((Pad * 2f) + TitleBarHeight)
            );

            Rect r = new(content.x, content.y, content.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(r, mode, new GUIContent("Mode"));
            r.y += r.height + Gap;

            if (hasIn)
            {
                float v = fadeIn.floatValue;
                v = EditorGUI.FloatField(r, new GUIContent("In (s)"), v);
                fadeIn.floatValue = Mathf.Max(0f, v);
                r.y += r.height + Gap;
            }

            if (hasOut)
            {
                float v = fadeOut.floatValue;
                v = EditorGUI.FloatField(r, new GUIContent("Out (s)"), v);
                fadeOut.floatValue = Mathf.Max(0f, v);
                r.y += r.height + Gap;
            }

            if (!hasIn && !hasOut)
            {
                EditorGUI.LabelField(r, "No fade will be applied.", EditorStyles.miniLabel);
            }

            EditorGUI.EndProperty();
        }

        private static bool HasIn(SerializedProperty mode)
        {
            if (mode == null)
            {
                return false;
            }

            int idx = mode.enumValueIndex;
            return idx == (int)FadeMode.In || idx == (int)FadeMode.InOut;
        }

        private static bool HasOut(SerializedProperty mode)
        {
            if (mode == null)
            {
                return false;
            }

            int idx = mode.enumValueIndex;
            return idx == (int)FadeMode.Out || idx == (int)FadeMode.InOut;
        }

        private static void DrawPanel(Rect rect, string title)
        {
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.08f));

            Handles.BeginGUI();
            Color prev = Handles.color;
            Handles.color = new Color(0f, 0f, 0f, 0.25f);
            Handles.DrawAAPolyLine(1f,
                new Vector3(rect.x, rect.y),
                new Vector3(rect.xMax, rect.y),
                new Vector3(rect.xMax, rect.yMax),
                new Vector3(rect.x, rect.yMax),
                new Vector3(rect.x, rect.y));
            Handles.color = prev;
            Handles.EndGUI();

            Rect bar = new(rect.x, rect.y, 3f, rect.height);
            EditorGUI.DrawRect(bar, Accent);

            Rect titleRect = new(rect.x + 8f, rect.y + 2f, rect.width - 8f, TitleBarHeight);
            EditorGUI.LabelField(titleRect, title, EditorStyles.miniBoldLabel);
        }
    }
}