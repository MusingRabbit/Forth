using Assets.Scripts.Actor;
using UnityEngine;

namespace Assets.Scripts.Events
{
    public class OnStateChangedEventArgs
    {
        public GameObject Actor { get; set; }
        public ActorState State { get; set; }
    }
}
