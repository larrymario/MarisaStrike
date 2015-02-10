using UnityEngine.EventSystems;

namespace MarisaStrike {

    public interface IDirector : IEventSystemHandler {

        void changeState(SceneInfo.SceneState state);

    }
}
