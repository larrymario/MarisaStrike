using UnityEngine.EventSystems;

namespace MarisaStrike {

    public interface ICharacter : IEventSystemHandler {

        int GetDamaged(int damage);
    }

}
