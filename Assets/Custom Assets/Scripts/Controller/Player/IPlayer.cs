using UnityEngine.EventSystems;

namespace MarisaStrike {

    public interface IPlayer : IEventSystemHandler {

        int getDamaged(int damage);
    }

}
