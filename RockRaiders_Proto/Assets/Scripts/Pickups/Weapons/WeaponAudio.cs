using Assets.Scripts;
using Assets.Scripts.Audio;
using Assets.Scripts.Network;

using UnityEngine;

public class WeaponAudio : RRAudioBehaviour
{
    public WeaponAudio()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public bool PlayRandomShotSound()
    {
        if (this.Sounds.Length < 1)
        {
            return false;
        }

        var rndIdx = Random.Range(0, this.Sounds.Length);
        var sound = this.Sounds[rndIdx];
        sound.Source.Play();

        return true;
    }
}
