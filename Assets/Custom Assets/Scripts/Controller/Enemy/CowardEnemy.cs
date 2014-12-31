using System;
using UnityEngine;
using System.Collections.Generic;

namespace MarisaStrike {

    public class CowardEnemy : MonoBehaviour, IEnemy {

        public int initialHP;
        public int turnTime;
        public float speed;
        public Rigidbody2D dropItem = null;

        private int HP;
        private EnemyInfo.State state;
        private int moveTimer;
        private bool isFacingLeft;
        private List<GameObject> detectedObjects;

        private Rigidbody2D enemyRigidbody;
        


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

            enemyRigidbody = GetComponent<Rigidbody2D>();
        }



        void Reset() {
            initialHP = 1;
        }



        void Update() {

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
                    if (moveTimer == 0) {
                        isFacingLeft = true;
                        enemyRigidbody.velocity = new Vector2(-speed, enemyRigidbody.velocity.y);
                        transform.rotation = Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z);
                    }
                    else if (moveTimer == turnTime) {
                        isFacingLeft = false;
                        enemyRigidbody.velocity = new Vector2(speed, enemyRigidbody.velocity.y);
                        transform.rotation = Quaternion.Euler(transform.rotation.x, 180, transform.rotation.z);
                    }
                    moveTimer++;

                    detectedObjects.RemoveAll(new Predicate<GameObject>(delegate(GameObject obj) {
                        if (obj == null) return true;
                        else return false;
                    }));

                    foreach (GameObject o in detectedObjects) {
                        if (o.tag == "Player") { 
                            state = EnemyInfo.State.Alert;
                            moveTimer = 0;
                            break;
                        }
                    }
                    detectedObjects.Remove(null);

                    if (moveTimer == turnTime * 2) moveTimer = 0;
                    break;

                case EnemyInfo.State.Alert:

                    state = EnemyInfo.State.Running;
                    break;

                case EnemyInfo.State.Running:
                    if (moveTimer == 0) {
                        isFacingLeft = !isFacingLeft;
                        transform.rotation = Quaternion.Euler(transform.rotation.x, isFacingLeft ? 0 : 180, transform.rotation.z);
                        GetComponentInChildren<Detector>().gameObject.SetActive(false);
                    }
                    enemyRigidbody.velocity = new Vector2(speed * 4, enemyRigidbody.velocity.y);

                    detectedObjects.RemoveAll(new Predicate<GameObject>(delegate(GameObject obj) {
                        if (obj == null) return true;
                        else return false;
                    }));

                    foreach (GameObject o in detectedObjects) {
                        if (o.tag == "Terrain") {
                            state = EnemyInfo.State.Shaking;
                            enemyRigidbody.velocity = new Vector2(0, enemyRigidbody.velocity.y);
                            moveTimer = 0;
                            break;
                        }
                    }

                    moveTimer++;
                    break;

                case EnemyInfo.State.Shaking:
                    if (moveTimer % 2 == 0) {
                        transform.Translate(0.5f, 0f, 0f, transform);
                    }
                    else {
                        transform.Translate(-0.5f, 0f, 0f, transform);
                    }
                    moveTimer++;
                    break;
            }
        }



        void processDeath() {
            if (dropItem != null)
                Instantiate(dropItem, transform.position, new Quaternion(0f, 0f, 0f, 0f));
            Destroy(gameObject);
        }



        void OnTriggerEnter2D(Collider2D other) {

            if (state == EnemyInfo.State.Running) {
                if (other.tag == "Terrain") {
                    state = EnemyInfo.State.Shaking;
                    enemyRigidbody.velocity = new Vector2(0, enemyRigidbody.velocity.y);
                    moveTimer = 0;
                }
            }
        }

    }

}