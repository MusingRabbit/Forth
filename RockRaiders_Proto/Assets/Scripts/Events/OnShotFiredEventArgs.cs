using System;
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
