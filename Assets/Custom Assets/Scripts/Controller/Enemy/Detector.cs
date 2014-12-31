using UnityEngine;
using UnityEngine.EventSystems;

namespace MarisaStrike {
    class Detector : MonoBehaviour {

        public GameObject target = null;

        void OnTriggerEnter2D(Collider2D other) {
            ExecuteEvents.Execute<IEnemy>(target, null, (x, y) => x.AddDetectedObject(other.gameObject));
        }

        void OnTriggerExit2D(Collider2D other) {
            ExecuteEvents.Execute<IEnemy>(target, null, (x, y) => x.RemoveDetectedObject(other.gameObject));
        }
    }
}
