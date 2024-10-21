using UnityEngine;

namespace Assets.Scripts
{
    public abstract class RRMonoBehaviour : MonoBehaviour, IInitialise
    {
        public abstract void Initialise();

        public abstract void Reset();
    }
}
