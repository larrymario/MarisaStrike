using System;
using UnityEngine;
using System.Collections.Generic;

namespace MarisaStrike {

    public class Enemy : MonoBehaviour, IEnemy {

        public int initialHP;
        public float speed;
        public Rigidbody2D dropItem = null;

        protected int HP;
        protected EnemyInfo.State state;
        protected int moveTimer;
        protected bool isFacingLeft;
        protected List<GameObject> detectedObjects;

        protected Rigidbody2D enemyRigidbody;



        public virtual int getDamaged(int damage) {
            HP -= damage;

            return HP;
        }



        public virtual void AddDetectedObject(GameObject obj) {
            detectedObjects.Add(obj);
        }



        public virtual void RemoveDetectedObject(GameObject obj) {
            detectedObjects.Remove(obj);
        }
        
    }

}