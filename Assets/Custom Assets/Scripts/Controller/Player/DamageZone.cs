﻿using UnityEngine;
using UnityEngine.EventSystems;

namespace MarisaStrike {

    public class DamageZone : MonoBehaviour {

        public int damageTime;
        public int animationTime;

        private int damage;
        private int damageTimer;
        private int animationTimer;



        public void setDamage(int damage) {
            this.damage = damage;
        }

        void Start() {
            damageTimer = 0;
            animationTimer = 0;
        }



        void Reset() {
            damageTime = 1;
            animationTime = 1;
        }



        void Update() {

        }



        void FixedUpdate() {

            damageTimer++;
            animationTimer++;

            if (animationTimer == animationTime) {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if ((other.tag == "Enemy")) {
                other.GetComponent<Enemy>().getDamaged(damage);
            }

        }
    }

}