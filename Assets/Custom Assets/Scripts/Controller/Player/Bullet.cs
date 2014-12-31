using UnityEngine;

namespace MarisaStrike {

    public class Bullet : MonoBehaviour {

        public int damage;
        public int speed;
        public int selfDestroyTime;            //Minus value available, meaning the bullet won't self-destroy
        public Rigidbody2D damageZone;

        private int selfDestroyTimer;



        void Reset() {
            selfDestroyTime = -1;
        }



        void Start() {
            selfDestroyTimer = 0;
        }



        void Update() {

        }



        void FixedUpdate() {
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