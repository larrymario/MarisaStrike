using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MarisaStrike {

    public class Director : MonoBehaviour, IDirector {

        public Rigidbody2D player;
        public Text startText;
        public Text clearText;
        public Text gameoverText;
        public RawImage mask;

        private CharacterInfo.SceneState state;
        private int timer;



        public void changeState(CharacterInfo.SceneState state) {
            this.state = state;
            
        }



        void Start() {
            startText.enabled = false;
            clearText.enabled = false;
            mask.color = new Color(0f, 0f, 0f, 1f);
            state = CharacterInfo.SceneState.Opening;
            timer = 0;
        }



        void OnGUI() {
            GUI.Label(new Rect(20, 275, 100, 30), state.ToString());
        }


        void FixedUpdate() {
            if (state != CharacterInfo.SceneState.Active) {
                switch (state) {
                    case CharacterInfo.SceneState.Opening:
                        if (timer < 60) {
                            mask.color = new Color(0f, 0f, 0f, mask.color.a - 0.01666f);
                        }
                        if (timer == 60) {
                            mask.enabled = false;
                            startText.enabled = true;
                        }
                        timer++;
                        if (timer == 150) {
                            startText.enabled = false;
                            player.GetComponent<Marisa>().setControllability(true);
                            state = CharacterInfo.SceneState.Active;
                            timer = 0;
                        }
                        break;
                    case CharacterInfo.SceneState.Clear:
                        if (timer == 0) {
                            player.GetComponent<Marisa>().setControllability(false);
                            clearText.enabled = true;
                        }
                        if (timer == 240) {
                            mask.enabled = true;
                        }
                        if (timer >= 240 && timer < 300) {
                            mask.color = new Color(0f, 0f, 0f, mask.color.a + 0.0166f);
                        }
                        if (timer == 300) {


                        }
                        timer++;
                        break;
                    case CharacterInfo.SceneState.GameOver:

                        break;
                }
            }


        }




    }

}