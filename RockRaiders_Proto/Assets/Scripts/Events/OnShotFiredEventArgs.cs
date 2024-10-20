using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Events
{
    public class OnShotFiredEventArgs : EventArgs
    {
        public Vector3 ProjectileVelocity { get; set; }
        public float ProjectileMass { get; set; }

        public OnShotFiredEventArgs(Vector3 velocity, float mass)
        {
            this.ProjectileVelocity = velocity;
            this.ProjectileMass = mass;
        }
    }
}
