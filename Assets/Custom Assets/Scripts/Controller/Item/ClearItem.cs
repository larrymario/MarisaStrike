using UnityEngine;
using UnityEngine.EventSystems;

namespace MarisaStrike {

    public class ClearItem : Item {

        public GameObject director;
        


        void Start() {
            
        }



        void FixedUpdate() {

        }



        void OnTriggerEnter2D(Collider2D other) {
            if (other.tag == "Player") {
                ExecuteEvents.Execute<IDirector>(director, null, (x, y) => x.changeState(SceneInfo.SceneState.Clear));
            }
            
        }


    }

}