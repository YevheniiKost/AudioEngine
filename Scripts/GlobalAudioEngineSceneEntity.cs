using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YellowTape.AudioEngine
{
    public static class MyAudio
    {
        public static IAudioEngine Engine { get; private set; }

          public static void SetEngine(IAudioEngine engine)
          {
              if (Engine == null)
                  Engine = engine;
              else
                  Debug.LogError("Audio Engine already assigned!");
          }
    }
    
    [DefaultExecutionOrder(-250)]
    public class GlobalAudioEngineSceneEntity : MonoBehaviour
    {
        public IAudioEngine AudioEngine => _engine;

        private IAudioEngine _engine;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            List<SfxParameters> parameters = GetParameters();
            Dictionary<SoundType, SfxEntity> dictionary = new Dictionary<SoundType, SfxEntity>();
            foreach (var config in parameters)
            {
                var source = gameObject.AddComponent<AudioSource>();
                SfxEntity entity = new SfxEntity(config, source, this);
                dictionary.Add(config.SoundType, entity);
            }

            _engine = new GlobalAudioEngine(dictionary);
            MyAudio.SetEngine(_engine);
        }

        public void StartCustomCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }

        private List<SfxParameters> GetParameters()
        {
            var config = AudioEngineHelper.LoadConfig();
            return config.Parameters;
        }
    }
}