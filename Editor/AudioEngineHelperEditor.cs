using UnityEditor;

namespace YellowTape.AudioEngine
{
    public static class AudioEngineHelperEditor
    {
        [MenuItem("Yellow Tape/Audio Engine/Select Config")]
        private static void NewMenuOption()
        {
            EditorGUIUtility.PingObject(AudioEngineHelper.LoadConfig());
        }
    }
}