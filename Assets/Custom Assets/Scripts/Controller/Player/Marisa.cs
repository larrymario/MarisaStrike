using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Data.SQLite;

namespace MarisaStrike {
    
    public class Marisa : MonoBehaviour, ICharacter {

        private const int MAX_QUEUE_COUNT = 20;

        public int HP;
        public float maxWalkSpeed;
        public float maxDashSpeed;
        public float jumpPower;
        public Vector2 repelForce = new Vector2(-4.5f, 5.4f);
        public LayerMask terrainLayer;

        private uint keyCode;
        private uint keyHoldTime;
        private List<uint> keyCodeQueue;
        private List<uint> keyHoldTimeQueue;
        private float horizontalInput;
        private float verticalInput;

        private CharState charMoveState;
        private CharState charJumpState;
        private CharState charFireState;
        private CharacterInfo.FireType currentFire;

        private bool isOnGround;
        private bool isInvincible;
        private bool isFacingLeft;
        private bool isControllable;

        private int fireTimer;
        private int fireCount;
        private int fireDelay;
        private int jumpTimer;
        private int repelTimer;
        private int invinTimer;

        private Animator playerAnimator;
        //private BoxCollider2D playerBoxCollider2D;
        private CircleCollider2D playerCircleCollider2D;
        private Rigidbody2D playerRigidbody2D;
        private SpriteRenderer playerSpriteRenderer;

        //Equipment related settings
        private Rigidbody2D normalFire;
        private int normalFireDelay;
        private Rigidbody2D weapon1Fire;
        private int weapon1FireDelay;
        private int weapon1FireCount;

        private int repelTime;
        private int invinTime;



        public int GetDamaged(int damage) {
            if ((!isInvincible) && (charMoveState != CharState.Damaged) && (charMoveState != CharState.Defeated)) {
                HP -= damage;
                if (HP > 0) {
                    charMoveState = CharState.Damaged;
                }
                else {
                    charMoveState = CharState.Defeated;
                }
            }
            return HP;
        }



        public void SetControllability(bool para) {
            isControllable = para;
            if (para == false) {
                for (int i = 0; i < MAX_QUEUE_COUNT; i++) {
                    keyCodeQueue.Remove(keyCodeQueue[0]);
                    keyHoldTimeQueue.Remove(keyHoldTimeQueue[0]);
                    keyCodeQueue.Add(0);
                    keyHoldTimeQueue.Add(16);
                }
                charMoveState = CharState.Stand;
                charJumpState = CharState.Stand;
                charFireState = CharState.Safe;
                horizontalInput = 0;
                verticalInput = 0;
            }
        }



        public void ChangeWeapon(CharacterInfo.FireType type) {
            currentFire = type;
            if (currentFire == CharacterInfo.FireType.Weapon1) {
                fireDelay = weapon1FireDelay;
                fireCount = weapon1FireCount;
            }
        }


        void Reset() {
            Awake();

            maxWalkSpeed = 12f;
            maxDashSpeed = 20f;
            jumpPower = 600f;
            repelForce = new Vector2(-4.5f, 5.4f);
            terrainLayer = 1 << LayerMask.NameToLayer("Terrain");
        }




        void Awake() {

            LoadEquipmentSettings();

            keyCode = 0;
            keyHoldTime = 0;
            keyCodeQueue = new List<uint>(MAX_QUEUE_COUNT);
            keyHoldTimeQueue = new List<uint>(MAX_QUEUE_COUNT);
            for (int i = 0; i < MAX_QUEUE_COUNT; i++) {
                keyCodeQueue.Add(0);
                keyHoldTimeQueue.Add(16);
            }

        }


        //Load equipmetnt settings from database

        void LoadEquipmentSettings() {
            normalFire = Resources.Load<Rigidbody2D>("BulletSmall");
            normalFireDelay = 8;
            weapon1Fire = Resources.Load<Rigidbody2D>("BulletSmall");
            weapon1FireDelay = 6;
            weapon1FireCount = 50;
            repelTime = 30;
            invinTime = 180;
        }




        void Start() {
            charMoveState = CharState.Stand;
            charJumpState = CharState.Stand;
            charFireState = CharState.Safe;
            currentFire = CharacterInfo.FireType.Normal;

            isOnGround = false;
            isInvincible = false;
            isFacingLeft = false;
            isControllable = false;

            fireTimer = 0;
            fireCount = 0;
            fireDelay = normalFireDelay;
            jumpTimer = 0;
            repelTimer = 0;
            invinTimer = 0;

            playerAnimator = GetComponent<Animator>();
            //playerBoxCollider2D = GetComponent<BoxCollider2D>();
            playerCircleCollider2D = GetComponent<CircleCollider2D>();
            playerRigidbody2D = GetComponent<Rigidbody2D>();
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
        }



        void OnGUI() {
            GUI.Box(new Rect(10, 10, 200, 300), "");
            GUI.Label(new Rect(20, 20, 100, 30), keyCode.ToString() + " " + keyHoldTime.ToString());

            GUI.Label(new Rect(20, 50, 100, 30), keyCodeQueue[MAX_QUEUE_COUNT - 1].ToString() + " " + keyHoldTimeQueue[MAX_QUEUE_COUNT - 1].ToString());
            GUI.Label(new Rect(20, 65, 100, 30), keyCodeQueue[MAX_QUEUE_COUNT - 2].ToString() + " " + keyHoldTimeQueue[MAX_QUEUE_COUNT - 2].ToString());
            GUI.Label(new Rect(20, 80, 100, 30), keyCodeQueue[MAX_QUEUE_COUNT - 3].ToString() + " " + keyHoldTimeQueue[MAX_QUEUE_COUNT - 3].ToString());
            GUI.Label(new Rect(20, 95, 100, 30), keyCodeQueue[MAX_QUEUE_COUNT - 4].ToString() + " " + keyHoldTimeQueue[MAX_QUEUE_COUNT - 4].ToString());
            GUI.Label(new Rect(20, 110, 100, 30), keyCodeQueue[MAX_QUEUE_COUNT - 5].ToString() + " " + keyHoldTimeQueue[MAX_QUEUE_COUNT - 5].ToString());
            GUI.Label(new Rect(20, 125, 100, 30), keyCodeQueue[MAX_QUEUE_COUNT - 6].ToString() + " " + keyHoldTimeQueue[MAX_QUEUE_COUNT - 6].ToString());
            GUI.Label(new Rect(20, 140, 100, 30), keyCodeQueue[MAX_QUEUE_COUNT - 7].ToString() + " " + keyHoldTimeQueue[MAX_QUEUE_COUNT - 7].ToString());
            GUI.Label(new Rect(20, 155, 100, 30), keyCodeQueue[MAX_QUEUE_COUNT - 8].ToString() + " " + keyHoldTimeQueue[MAX_QUEUE_COUNT - 8].ToString());
            GUI.Label(new Rect(20, 170, 100, 30), keyCodeQueue[MAX_QUEUE_COUNT - 9].ToString() + " " + keyHoldTimeQueue[MAX_QUEUE_COUNT - 9].ToString());
            GUI.Label(new Rect(20, 185, 100, 30), keyCodeQueue[MAX_QUEUE_COUNT - 10].ToString() + " " + keyHoldTimeQueue[MAX_QUEUE_COUNT - 10].ToString());

            GUI.Label(new Rect(20, 215, 100, 30), charMoveState.ToString());
            GUI.Label(new Rect(20, 230, 100, 30), charJumpState.ToString());
            GUI.Label(new Rect(20, 245, 100, 30), charFireState.ToString());
            GUI.Label(new Rect(20, 260, 100, 30), fireCount.ToString());
        }



        void Update() {

        }
        


        void FixedUpdate() {
            if (isControllable) {
                GetInput();
            }
            ProcessInput();
            CheckIsOnGround();
        }
        


        void GetInput() {
            uint keyCodeTemp = 0;

            // keyCode is a bitmap:
            //
            // Up       Down     Forward  Backward Reserved Reserved Fire     Jump
            // 0        0        0        0        0        0        0        0
            //
            // 0 for "not pressed", 1 for "pressed".

            if (Input.GetAxis("Fire1") > 0.001f) {              //Fire
                keyCodeTemp |= (uint)KeyMap.Fire;
            }

            if (Input.GetAxis("Jump") > 0.001f) {               //Jump
                keyCodeTemp |= (uint)KeyMap.Jump;
            }

            horizontalInput = Input.GetAxis("Horizontal");
            if (Input.GetAxis("Horizontal") > 0.001f) {         //Forward OR Backward
                if (isFacingLeft) {
                    keyCodeTemp |= (uint)KeyMap.Backward;       //Press Right when facing Left
                }
                else {
                    keyCodeTemp |= (uint)KeyMap.Forward;        //Press Left when facign Left
                }
            }
            else if (Input.GetAxis("Horizontal") < -0.001f) {
                if (isFacingLeft) {
                    keyCodeTemp |= (uint)KeyMap.Forward;        //Press Right when facing Right
                }
                else {
                    keyCodeTemp |= (uint)KeyMap.Backward;       //Press Left when facing Right
                }
            }

            verticalInput = Input.GetAxis("Vertical");
            if (Input.GetAxis("Vertical") > 0.001f) {           //Up OR Down
                keyCodeTemp |= (uint)KeyMap.Up;
            }
            else if (Input.GetAxis("Vertical") < -0.001f) {
                keyCodeTemp |= (uint)KeyMap.Down;
            }

            if (keyCode != keyCodeTemp) {                       //if key changed...
                keyCodeQueue.Remove(keyCodeQueue[0]);
                keyHoldTimeQueue.Remove(keyHoldTimeQueue[0]);
                keyCodeQueue.Add(keyCode);                      //Push the previous key into the queue
                keyHoldTimeQueue.Add(keyHoldTime);
                keyHoldTime = 0;                                //And reset
            }

            if (keyHoldTime < 1000) keyHoldTime++;
            keyCode = keyCodeTemp;                              //Send keyCode
        }



        void ProcessInput() {

            if ((charMoveState != CharState.Damaged) && (charMoveState != CharState.Defeated)) {

                //charMoveState

                if ((keyCode & (uint)KeyMap.Forward) > 0) {
                    charMoveState = isOnGround ? CharState.Walk : CharState.AirWalk;

                    if ((keyCodeQueue[MAX_QUEUE_COUNT - 1] == (uint)KeyMap.Idle) &&             //Dash: Short Forward -> Short Idle -> Forward
                        (keyHoldTimeQueue[MAX_QUEUE_COUNT - 1] <= 8)) {
                        if (((keyCodeQueue[MAX_QUEUE_COUNT - 2] & (uint)KeyMap.Forward) > 0) &&
                            (keyHoldTimeQueue[MAX_QUEUE_COUNT - 2] <= 8)) {
                            charMoveState = isOnGround ? CharState.Dash : CharState.AirDash;
                        }
                    }

                }
                else if ((keyCode & (uint)KeyMap.Backward) > 0) {
                    charMoveState = CharState.Turning;
                }

                if ((keyCode & 254) == 0)                                       //254 -> 11111110
                {
                    charMoveState = isOnGround ? CharState.Stand : CharState.Air;

                }

                //charJumpState

                if ((keyCode & (uint)KeyMap.Jump) > 0) {
                    if (isOnGround)
                        charJumpState = CharState.Jump;
                }
                else {
                    charJumpState = CharState.Stand;
                }




                //charFireState

                if ((keyCode & (uint)KeyMap.Fire) > 0) {
                    charFireState = CharState.Fire;
                }
                else {
                    charFireState = CharState.Safe;
                }

            }

            //State Processing

            switch (charMoveState) {
                case CharState.Stand:
                    playerRigidbody2D.velocity = new Vector2(0, playerRigidbody2D.velocity.y);
                    break;
                case CharState.Walk:
                    playerRigidbody2D.velocity = new Vector2(horizontalInput * maxWalkSpeed,
                        playerRigidbody2D.velocity.y);
                    break;
                case CharState.AirDash:
                    playerRigidbody2D.velocity = new Vector2(horizontalInput * maxDashSpeed,
                    playerRigidbody2D.velocity.y);
                    break;
                case CharState.AirWalk:
                    playerRigidbody2D.velocity = new Vector2(horizontalInput * maxWalkSpeed,
                    playerRigidbody2D.velocity.y);
                    break;
                case CharState.Dash:
                    playerRigidbody2D.velocity = new Vector2(horizontalInput * maxDashSpeed,
                        playerRigidbody2D.velocity.y);
                    break;
                case CharState.Turning:
                    isFacingLeft = !isFacingLeft;
                    transform.rotation = Quaternion.Euler(transform.rotation.x, isFacingLeft ? 180 : 0, transform.rotation.z);
                    break;
                case CharState.Air:
                    playerRigidbody2D.velocity = new Vector2(0, playerRigidbody2D.velocity.y);
                    break;
                case CharState.Damaged:
                    if (repelTimer == 0) {
                        charJumpState = CharState.Stand;
                        charFireState = CharState.Safe;
                        isInvincible = true;
                        playerRigidbody2D.velocity = new Vector2(transform.right.x * repelForce.x, transform.up.y * repelForce.y);

                        playerAnimator.SetTrigger("Damaged");
                    }
                    repelTimer++;
                    if (repelTimer == repelTime) {
                        charMoveState = CharState.Stand;
                        playerAnimator.SetTrigger("repelFinish");
                        repelTimer = 0;
                    }
                    

                    break;
                case CharState.Defeated:

                    break;
            }

            switch (charJumpState) {
                case CharState.Jump:
                    if (jumpTimer < 3) {
                        if (jumpTimer == 0) {
                            playerAnimator.SetTrigger("Jump");
                        }

                        playerRigidbody2D.AddForce(Vector2.up * jumpPower);
                        switch (charMoveState) {
                            case CharState.Stand:
                                charMoveState = CharState.Air;
                                break;
                            case CharState.Walk:
                                charMoveState = CharState.AirWalk;
                                break;
                            case CharState.Dash:
                                charMoveState = CharState.AirDash;
                                break;

                        }

                        jumpTimer++;
                    }
                    break;
                
                case CharState.Stand:
                    jumpTimer = 0;
                    break;
            }

            switch (charFireState) {
                case CharState.Safe:
                    playerAnimator.SetBool("Fire", false);
                    if (fireTimer != normalFireDelay) fireTimer = 0;
                    break;
                case CharState.Fire:
                    playerAnimator.SetBool("Fire", true);
                    if (fireTimer == 0) {
                        GenerateBullet(currentFire);
                        if (currentFire != CharacterInfo.FireType.Normal) {
                            fireCount--;
                            if (fireCount == 0) {
                                currentFire = CharacterInfo.FireType.Normal;
                                fireDelay = normalFireDelay;
                            }
                        }
                    }
                    fireTimer++;
                    if (fireTimer == fireDelay) fireTimer = 0;
                    break;

                    
            }

            //Speed Limitation
            if (playerRigidbody2D.velocity.y > 20f) {
                playerRigidbody2D.velocity = new Vector2(playerRigidbody2D.velocity.x, 20f);
            }
            if (playerRigidbody2D.velocity.y < -20f) {
                playerRigidbody2D.velocity = new Vector2(playerRigidbody2D.velocity.x, -20f);
            }



            //Invincibility handling

            if (isInvincible) {
                if (invinTimer % 2 == 0) {
                    playerSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);
                    
                }
                else {
                    playerSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                }
                invinTimer++;
                if (invinTimer == invinTime) {
                    playerSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                    isInvincible = false;
                    invinTimer = 0;
                }
            }

            //Animation related

            playerAnimator.SetFloat("Horizontal", playerRigidbody2D.velocity.x);
            playerAnimator.SetFloat("Vertical", playerRigidbody2D.velocity.y);
        }



        void CheckIsOnGround() {
            Vector2 pos = transform.position;
            Vector2 leftup = new Vector2(pos.x + playerCircleCollider2D.center.x - 0.98f * playerCircleCollider2D.radius,
                                        pos.y + playerCircleCollider2D.center.y);
            Vector2 rightdown = new Vector2(pos.x + playerCircleCollider2D.center.x + 0.98f * playerCircleCollider2D.radius,
                                        pos.y + playerCircleCollider2D.center.y - 1.1f * playerCircleCollider2D.radius);

            isOnGround = Physics2D.OverlapArea(leftup, rightdown, terrainLayer);
            playerAnimator.SetBool("isOnGround", isOnGround);
        }



        void GenerateBullet(CharacterInfo.FireType type) {
            switch (type) {
                case CharacterInfo.FireType.Normal:
                    Rigidbody2D bulletClone = (Rigidbody2D)Instantiate(normalFire, transform.position, transform.rotation);
                    bulletClone.velocity = new Vector2(isFacingLeft ? -1 : 1, 0) * normalFire.GetComponent<Bullet>().speed;
                    break;
                case CharacterInfo.FireType.Weapon1:
                    Rigidbody2D bulletClone1 = (Rigidbody2D)Instantiate(weapon1Fire, 
                        new Vector2(transform.position.x, transform.position.y + 0.4f), transform.rotation);
                    Rigidbody2D bulletClone2 = (Rigidbody2D)Instantiate(weapon1Fire, 
                        new Vector2(transform.position.x, transform.position.y - 0.4f), transform.rotation);
                    bulletClone1.velocity = new Vector2(isFacingLeft ? -1 : 1, 0) * weapon1Fire.GetComponent<Bullet>().speed;
                    bulletClone2.velocity = new Vector2(isFacingLeft ? -1 : 1, 0) * weapon1Fire.GetComponent<Bullet>().speed;
                    break;
            }
        }
        


        //Enums

        enum CharState {
            Stand,
            Crouch,
            Walk,
            Turning,
            Shoot,
            Dash,
            Air,
            AirWalk,
            AirDash,

            Damaged,
            Defeated,

            Jump,

            Safe,
            Fire
        };

        enum KeyMap {
            Up = 128,
            Down = 64,
            Forward = 32,
            Backward = 16,
            Reserve1 = 8,
            Reserve2 = 4,
            Fire = 2,
            Jump = 1,
            Idle = 0
        };


    }


}