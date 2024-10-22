using Assets.Scripts.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Events
{
    public class OnStateChangedEventArgs
    {
        public GameObject Actor { get; set; }
        public ActorState State { get; set; }
    }
}
