using UnityEngine;
using UnityEngine.EventSystems;

namespace MarisaStrike {

    public interface IEnemy : IEventSystemHandler {

        int getDamaged(int damage);
        void AddDetectedObject(GameObject obj);
        void RemoveDetectedObject(GameObject obj);

    }

}