using Assets.Scripts.Audio;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Actor
{
    public class ActorAudio : RRAudioBehaviour
    {
        private HashSet<string> m_ouchSounds = new HashSet<string> { "Ouch1", "Ouch2" };

        public ActorAudio()
        {

        }

        public bool PlayRandomOuchSound()
        {
            if (this.Sounds.Length < 1)
            {
                return false;
            }

            var ouchSounds = this.Sounds.Where(x => !m_ouchSounds.Contains(x.Name)).ToList();

            if (!ouchSounds.Any())
            {
                return false;
            }

            var rndIdx = Random.Range(0, ouchSounds.Count);
            var sound = this.Sounds[rndIdx];
            sound.Source.Play();
            return true;
        }
    }
}
