using UnityEngine;

namespace MarisaStrike {

    public class HGBullet : Bullet {

        void Start() {
            base.Start();
        }

        void FixedUpdate() {
            if (!isAdjusted) {
                Vector3 speed3D = new Vector3(1, 0, 0);
                speed3D = rotation * speed3D;
                bulletRigidbody2D.velocity = new Vector2(speed3D.x, speed3D.y) * this.speed;
                isAdjusted = true;
            }

            if (selfDestroyTime > 0) {
                selfDestroyTimer++;
                if (selfDestroyTimer == selfDestroyTime) {
                    Explode();
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if ((other.tag == "Terrain") || (other.tag == "Enemy")) {
                Explode();
                return;
            }

            if (other.tag == "Border") {
                Destroy(gameObject);                //Touching borders will not trigger damage zone
                return;
            }

        }

        void Explode() {
            Rigidbody2D damageZoneClone = (Rigidbody2D)Instantiate(damageZone, transform.position, transform.rotation);
            damageZoneClone.gameObject.GetComponent<DamageZone>().setDamage(damage);
            Destroy(gameObject);
        }
    }

}