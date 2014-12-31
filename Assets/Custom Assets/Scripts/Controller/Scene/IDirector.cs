using UnityEngine.EventSystems;

namespace MarisaStrike {

    public interface IDirector : IEventSystemHandler {

        void changeState(CharacterInfo.SceneState state);
    }
}
