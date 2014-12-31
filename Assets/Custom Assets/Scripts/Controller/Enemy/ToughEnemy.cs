using System;
using UnityEngine;
using System.Collections.Generic;

namespace MarisaStrike {

    public class ToughEnemy : MonoBehaviour, IEnemy {

        public int initialHP;
        public float speed;
        public Rigidbody2D bullet;
        public int fireDelay;
        public Rigidbody2D dropItem = null;

        private int HP;
        private EnemyInfo.State state;
        private int moveTimer;
        private bool isFacingLeft;
        private List<GameObject> detectedObjects;
        private GameObject player;

        //private Rigidbody2D enemyRigidbody;



        public int getDamaged(int damage) {
            HP -= damage;
            return HP;
        }



        public void AddDetectedObject(GameObject obj) {
            detectedObjects.Add(obj);
        }



        public void RemoveDetectedObject(GameObject obj) {
            detectedObjects.Remove(obj);
        }



        void Start() {
            HP = initialHP;
            state = EnemyInfo.State.Wandering;
            moveTimer = 0;
            isFacingLeft = true;
            detectedObjects = new List<GameObject>();

            //enemyRigidbody = GetComponent<Rigidbody2D>();
        }



        void Reset() {
            initialHP = 1;
        }



        void FixedUpdate() {

            //Death Processing

            if (HP <= 0) {
                if (state != EnemyInfo.State.Dying) {
                    state = EnemyInfo.State.Dying;
                    processDeath();
                }
            }

            //AI
            switch (state) {
                case EnemyInfo.State.Wandering:

                    detectedObjects.RemoveAll(new Predicate<GameObject>(delegate(GameObject obj) {
                        if (obj == null) return true;
                        else return false;
                    }));

                    foreach (GameObject o in detectedObjects) {
                        if (o.tag == "Player") {
                            player = o;
                            state = EnemyInfo.State.Alert;
                            moveTimer = 0;
                            break;
                        }
                    }
                    
                    break;

                case EnemyInfo.State.Alert:
                    moveTimer++;
                    if (moveTimer == 30) {
                        moveTimer = 0;
                        state = EnemyInfo.State.Attacking;
                    }
                    break;

                case EnemyInfo.State.Attacking:
                    if (moveTimer == 0) GenerateBullet();
                    if (transform.position.x - player.transform.position.x > 0) isFacingLeft = true;
                    else isFacingLeft = false;
                    transform.rotation = new Quaternion(0, isFacingLeft ? 0 : 180, 0, 0);
                    moveTimer++;
                    if (moveTimer == fireDelay) moveTimer = 0;
                    break;
            }


        }



        void GenerateBullet() {
            Vector2 direction = (player.transform.position - transform.position).normalized;

            Rigidbody2D bulletClone = (Rigidbody2D)Instantiate(bullet, transform.position, transform.rotation);
            bulletClone.velocity = direction * bullet.gameObject.GetComponent<EnemyBullet>().speed;

        }

        
        void processDeath() {
            if (dropItem != null)
                Instantiate(dropItem, transform.position, new Quaternion(0f, 0f, 0f, 0f));
            Destroy(gameObject);
        }



        void OnTriggerExit2D(Collider2D other) {
            if (other.tag == "Player") {
                state = EnemyInfo.State.Wandering;
                player = null;
            }
        }

    }

}