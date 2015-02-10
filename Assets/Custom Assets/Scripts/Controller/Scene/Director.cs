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

        private SceneInfo.SceneState state;
        private int timer;



        public void changeState(SceneInfo.SceneState state) {
            this.state = state;
            
        }



        void Start() {
            startText.enabled = false;
            clearText.enabled = false;
            mask.color = new Color(0f, 0f, 0f, 1f);
            state = SceneInfo.SceneState.Opening;
            timer = 0;
        }



        void OnGUI() {
            GUI.Label(new Rect(20, 275, 100, 30), state.ToString());
        }


        void FixedUpdate() {
            if (state != SceneInfo.SceneState.Active) {
                switch (state) {
                    case SceneInfo.SceneState.Opening:
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
                            player.GetComponent<Marisa>().SetControllability(true);
                            state = SceneInfo.SceneState.Active;
                            timer = 0;
                        }
                        break;
                    case SceneInfo.SceneState.Clear:
                        if (timer == 0) {
                            player.GetComponent<Marisa>().SetControllability(false);
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
                    case SceneInfo.SceneState.GameOver:

                        break;
                }
            }


        }




    }


}