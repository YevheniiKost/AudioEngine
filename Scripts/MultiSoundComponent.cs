using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace YellowTape.AudioEngine
{
    public class MultiSoundComponent : MonoBehaviour
    {
      public List<SoundComponentEntity> SoundComponentEntities = new List<SoundComponentEntity>();
      
      public void PlayByIndex(int index)
      {
          if(SoundComponentEntities.Count <= index)
              return;
          var entity = SoundComponentEntities[index];
          MyAudio.Engine.PlayClip(entity.SoundType, entity.Name);
      }
    }

    [System.Serializable]
    public class SoundComponentEntity
    {
        public SoundType SoundType;
        public string Name;
    }
}