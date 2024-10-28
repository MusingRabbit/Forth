using UnityEngine;

namespace Assets.Scripts.HealthSystem
{
    public interface IHealth
    {
        Hitpoints Hitpoints { get; }
        Damage LastDamage { get; }

        void Initialise();
        void RegisterDamage(Damage damage);
        void RegisterRigidBodyCollision(Rigidbody rigidBody);
        void RegisterRigidBodyCollisionWithDamage(Rigidbody rhsRigidBody, Damage rhsDamage);
        void Reset();
    }
}