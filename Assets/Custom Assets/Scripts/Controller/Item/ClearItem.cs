using UnityEngine;
using UnityEngine.EventSystems;

namespace MarisaStrike {

    public class ClearItem : MonoBehaviour {

        public GameObject director;

        //private bool isMovingUp;
        //private Rigidbody2D itemRigidbody;



        void Start() {
            //isMovingUp = true;
            //itemRigidbody = GetComponent<Rigidbody2D>();
        }



        void FixedUpdate() {

        }



        void OnTriggerEnter2D(Collider2D other) {
            if (other.tag == "Player") {
                ExecuteEvents.Execute<IDirector>(director, null, (x, y) => x.changeState(CharacterInfo.SceneState.Clear));
            }
            //Destroy(gameObject);
        }


    }

}