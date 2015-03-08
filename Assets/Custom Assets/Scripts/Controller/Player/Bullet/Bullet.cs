using UnityEngine;

namespace MarisaStrike {

    public class Bullet : MonoBehaviour {

        public int damage;
        public int speed;
        public int selfDestroyTime;            //Minus value available, meaning the bullet won't self-destroy
        public Rigidbody2D damageZone;
        public Quaternion rotation;
        
        protected int selfDestroyTimer;
        protected bool isAdjusted;
        protected Rigidbody2D bulletRigidbody2D;

        public void setDirection(Quaternion rotation) {
            this.rotation = rotation;
        }

        protected void Start() {
            selfDestroyTimer = 0;
            isAdjusted = false;
            bulletRigidbody2D = this.GetComponent<Rigidbody2D>();
        }
        
    }

}