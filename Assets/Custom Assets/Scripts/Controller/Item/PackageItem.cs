using UnityEngine;

namespace MarisaStrike {

    public class PackageItem : Item {

        public string equipmentName;

        private bool isMovingUp;



        void Start() {
            isMovingUp = true;
            itemRigidbody = GetComponent<Rigidbody2D>();
        }



        void FixedUpdate() {
            //Floating Effect

            if (isMovingUp)
                itemRigidbody.velocity = new Vector2(itemRigidbody.velocity.x, itemRigidbody.velocity.y + 0.1f);
            else
                itemRigidbody.velocity = new Vector2(itemRigidbody.velocity.x, itemRigidbody.velocity.y - 0.1f);

            if (itemRigidbody.velocity.y >= 2f) isMovingUp = !isMovingUp;
            if (itemRigidbody.velocity.y <= -2f) isMovingUp = !isMovingUp;
        }



        void OnTriggerEnter2D(Collider2D other) {
            if (other.tag == "Player") {

                Destroy(gameObject);
            }
            
        }


    }

}