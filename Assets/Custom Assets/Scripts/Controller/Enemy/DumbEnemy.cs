using UnityEngine;
using System.Collections;

namespace MarisaStrike {

    public class DumbEnemy : MonoBehaviour, IEnemy {

        public int initialHP;
        public int turnTime;
        public float speed;
        public Rigidbody2D dropItem = null;

        private int HP;
        private EnemyInfo.State state;
        private int moveTimer;
        //private bool isFacingLeft;

        private Rigidbody2D enemyRigidbody;



        public int getDamaged(int damage) {
            HP -= damage;
            return HP;
        }



        public void AddDetectedObject(GameObject obj) { }



        public void RemoveDetectedObject(GameObject obj) { }



        void Start() {
            HP = initialHP;
            state = EnemyInfo.State.Wandering;
            moveTimer = 0;
            //isFacingLeft = true;

            enemyRigidbody = GetComponent<Rigidbody2D>();

        }



        void Reset() {
            initialHP = 1;
        }



        void Update() {

        }



        void FixedUpdate() {

            //AI

            if (moveTimer == 0) {
                //isFacingLeft = true;
                enemyRigidbody.velocity = new Vector2(-speed, enemyRigidbody.velocity.y);
                transform.rotation = Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z);
            }
            else if (moveTimer == turnTime) {
                //isFacingLeft = false;
                enemyRigidbody.velocity = new Vector2(speed, enemyRigidbody.velocity.y);
                transform.rotation = Quaternion.Euler(transform.rotation.x, 180, transform.rotation.z);
            }

            moveTimer++;
            if (moveTimer == turnTime * 2) moveTimer = 0;


            //Death Processing

            if (HP <= 0) {
                if (state != EnemyInfo.State.Dying) {
                    state = EnemyInfo.State.Dying;
                    processDeath();
                }
            }

        }



        void processDeath() {
            if (dropItem != null)
                Instantiate(dropItem, transform.position, transform.rotation);
            Destroy(gameObject);
        }

    }

}