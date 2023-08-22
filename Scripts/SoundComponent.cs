using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace YellowTape.AudioEngine
{
    public class SoundComponent : MonoBehaviour
    {
        public SoundType SoundType;
        public string Name = "";
        
        private float _soundLength;
        private Coroutine _playCoroutine;

        public void Play()
        {
            if (_playCoroutine != null)
                StopCoroutine(_playCoroutine);
            _playCoroutine = StartCoroutine(PlayCoroutine());
        }
        
        private void Start()
        {
            _soundLength = MyAudio.Engine.GetClipLength(SoundType, Name);
        }

        private IEnumerator PlayCoroutine()
        {
            MyAudio.Engine.PlayClip(SoundType, Name);
            yield return new WaitForSeconds(_soundLength);
            _playCoroutine = null;
        }
        
#if UNITY_EDITOR
        public void AutoConnectToButtonComponent()
        {
            var button = GetComponent<Button>();
            if (button == null)
                return;
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, Play);
        }
#endif
        
    }
}