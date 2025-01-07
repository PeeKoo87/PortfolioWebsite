using Player.Combat.Weapons;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using AudioSystem;
using DefaultNamespace;

namespace Cyber_Slicer_AI
{
    
    public class AI_Shooter_CS : MonoBehaviour
    {
        [Header("General")]
        [Space]
        public Transform shootPoint; // Where the raycast starts from
        public Transform gunPoint; // Where the visual trail starts from
        public LayerMask layerMask; // The layermask to mask
        public SoundData shootSound;
        [Header("Gun")]
        [Space]
        public Vector3 spread = new Vector3(0.06f, 0.06f, 0.06f); // original: public Vector3 spread = new Vector3(0.06f, 0.06f, 0.06f);
        public TrailRenderer bulletTrail;

        public Vector2Int bulletDamageRange;
        public int ammo = 30;
        public int currentAmmo;

        Laser laser;
        //public int damage = 10;

        //new from enemy config, needs debugging.
        //public int ammo { get; set; }
        //public int currentAmmo;
        //public int damage { get; set; }

        [SerializeField] BulletInfo bulletInfo;

        Enemy enemy;
        public SoundData FootStepSound;

        void Awake()
        {
            enemy = transform.parent.GetComponent<Enemy>();
            laser = GetComponentInChildren<Laser>(); // sniper laser trace
            Reload();
        }
        private void Start()
        {
            //enemy.weaponIK.enabled = true;
        }

        public void Shoot()
        {
            if (ShouldReload())
            {
                //Debug.LogWarning("Ammo depleted! Reload required.");
                return;
            }

            var newBullet = BulletPool.GetBullet();
            if (newBullet == null)
            {
                Debug.LogError("No bullet available from pool!");
                return;
            }

            newBullet.OnHit += BulletHit;
            newBullet.OnReturnToPool += UnsubscribeBulletEvents;

            Vector3 direction = GetDirection();
             
            //Debug.DrawRay(shootPoint.position, direction * 10f, Color.blue, 1f);

            //Debug.DrawRay(shootPoint.position, shootPoint.forward * 10f, Color.green, 2f);

            //newBullet.Shoot(gunPoint.position, direction, bulletInfo);
            if (Physics.Raycast(shootPoint.position, direction, out RaycastHit hit, float.MaxValue, layerMask))
            {
                //Debug.Log($"Raycast hit: {hit.collider.name}");
                newBullet.Shoot(gunPoint.position, direction, bulletInfo);

                SoundManager.Instance.CreateSound()
                    .WithSoundData(shootSound)
                    .WithPosition(gunPoint.position)
                    .WithRandomPitch()
                    .Play();

                currentAmmo -= 1;
                //Debug.Log($"Shot fired. Remaining Ammo: {currentAmmo}");
            }
            else
            {
                //Debug.LogWarning("Raycast missed.");
                UnsubscribeBulletEvents(newBullet);
                BulletPool.ReturnBullet(newBullet);
            }
        }


        private void UnsubscribeBulletEvents(Bullet bullet)
        {
            bullet.OnHit -= BulletHit;
            bullet.OnReturnToPool -= UnsubscribeBulletEvents;
        }

        private void BulletHit(Bullet bullet, RaycastHit hit, IDamageable damageable)
        {
            int randomizedBulletDamage = Random.Range(bulletDamageRange.x, bulletDamageRange.y + 1);
            randomizedBulletDamage = Mathf.RoundToInt(randomizedBulletDamage * (1 + enemy.Stats.Damage * 0.01f));
            if (damageable is not PlayerUnit player) return;

            var damage = new Damage
            {
                Amount = randomizedBulletDamage,
                Type = DamageType.Bullet
            };
            player.Damage(ref damage);
        }

        public bool ShouldReload()
        {
            return currentAmmo <= 0;
        }

        public void Reload()
        {
            currentAmmo = ammo;
        }

        private Vector3 GetDirection()
        {
            Vector3 direction = gunPoint.transform.forward;
            direction += new Vector3(
                Random.Range(-spread.x, spread.x),
                Random.Range(-spread.y, spread.y),
                Random.Range(-spread.z, spread.z)
            );
            direction.Normalize();
            return direction;
        }

        public void enableIK()
        {
            // Toggle the IK state
            if (enemy.weaponIK != null)
            {
                enemy.weaponIK.enabled = !enemy.weaponIK.enabled;
            }
        }

        public void EnableTrace()
        {
            laser.EnableLineTrace();
        }

        public void DisableTrace()
        {
            laser.DisableLineTrace();
        }

        public void Footstep()
        {
            SoundManager.Instance.CreateSound()
                .WithSoundData(FootStepSound)
                .WithPosition(transform.position)
                .WithRandomPitch()
                .Play();
        }
    }
}
