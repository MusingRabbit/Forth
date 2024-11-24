using Assets.Scripts.Audio;


namespace Assets.Scripts.Pickups.Weapons.Projectiles
{
    public class ProjectileAudio : RRAudioBehaviour
    {
        public ProjectileAudio()
        {
            
        }

        protected override void Awake()
        {
            base.Awake();

            this.PlayAllSounds();
        }
    }
}
