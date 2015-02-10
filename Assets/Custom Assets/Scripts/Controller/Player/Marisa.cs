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

        private MoveState charMoveState;
        private JumpState charJumpState;
        private MoveState charFireState;
        private DirectionState charDirectionState;
        private DamageState charDamageState;

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
            if ((!isInvincible) && (charMoveState != MoveState.Damaged) && (charMoveState != MoveState.Defeated)) {
                HP -= damage;
                if (HP > 0) {
                    charMoveState = MoveState.Damaged;
                }
                else {
                    charMoveState = MoveState.Defeated;
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
                charMoveState = MoveState.Stand;
                charJumpState = JumpState.Idle;
                charFireState = MoveState.Safe;
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
            charMoveState = MoveState.Stand;
            charJumpState = JumpState.Idle;
            charFireState = MoveState.Safe;
            charDamageState = DamageState.Ok;
            charDirectionState = DirectionState.Forward;
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

            // keyCode bitmap:
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
            keyCode = keyCodeTemp;
        }
        
        void ProcessInput() {

            if ((charMoveState != MoveState.Damaged) && (charMoveState != MoveState.Defeated)) {

                /*
                //charMoveState

                if ((keyCode & (uint)KeyMap.Forward) > 0) {
                    charMoveState = isOnGround ? MoveState.Walk : MoveState.AirWalk;

                    if ((keyCodeQueue[MAX_QUEUE_COUNT - 1] == (uint)KeyMap.Idle) &&             //Dash: Short Forward -> Short Idle -> Forward
                        (keyHoldTimeQueue[MAX_QUEUE_COUNT - 1] <= 8)) {
                        if (((keyCodeQueue[MAX_QUEUE_COUNT - 2] & (uint)KeyMap.Forward) > 0) &&
                            (keyHoldTimeQueue[MAX_QUEUE_COUNT - 2] <= 8)) {
                            charMoveState = isOnGround ? MoveState.Dash : MoveState.AirDash;
                        }
                    }

                }
                else if ((keyCode & (uint)KeyMap.Backward) > 0) {
                    //charMoveState = MoveState.Turning;
                }

                if ((keyCode & 254) == 0)                                       //254 -> 11111110
                {
                    //charMoveState = isOnGround ? MoveState.Stand : MoveState.Air;

                }
                */





                //charFireState

                if ((keyCode & (uint)KeyMap.Fire) > 0) {
                    charFireState = MoveState.Fire;
                }
                else {
                    charFireState = MoveState.Safe;
                }

            }

            //New
            
            //charMoveState
            switch (charMoveState) {
                case MoveState.Stand:
                    if (isKeyDown(KeyMap.Forward)) {
                        charMoveState = MoveState.Walk;
                        if (isKeyIdle(48, 1) && isHoldTimeLessThan(8, 1)    //00110000
                            && isKeyDown(KeyMap.Forward, 2) && isHoldTimeLessThan(8, 1)) {
                                charMoveState = MoveState.Dash;
                        }
                    }
                    else if (!isOnGround) {
                        charMoveState = MoveState.Air;
                    }
                    break;
                case MoveState.Walk:
                    if (isKeyIdle(48)) {
                        charMoveState = MoveState.Stand;
                    }
                    else if (!isOnGround) {
                        charMoveState = MoveState.AirWalk;
                    }
                    break;
                case MoveState.Air:
                    if (isKeyDown(KeyMap.Forward)) {
                        charMoveState = MoveState.AirWalk;
                    }
                    else if (isOnGround) {
                        charMoveState = MoveState.Stand;
                    }
                    break;
                case MoveState.AirWalk:
                    if (isKeyIdle(48)) {
                        charMoveState = MoveState.Air;
                    }
                    else if (isOnGround) {
                        charMoveState = MoveState.Walk;
                    }
                    break;
                case MoveState.Dash:
                    if (isKeyIdle(48)) {
                        charMoveState = MoveState.Stand;
                    }
                    else if (!isOnGround) {
                        charMoveState = MoveState.AirWalk;
                    }
                    break;
            }

            //charJumpState
            if (isKeyDown(KeyMap.Jump)) {
                if (isOnGround)
                    charJumpState = JumpState.Jump;
            }
            else {
                charJumpState = JumpState.Idle;
            }

            //charDirectionState
            switch (charDirectionState) {
                case DirectionState.Forward:
                    if (isKeyDown(KeyMap.Up)) charDirectionState = DirectionState.Up;
                    if (isKeyDown(KeyMap.Down)) charDirectionState = DirectionState.Down;
                    if (isKeyDown(KeyMap.Backward)) charDirectionState = DirectionState.Backward;
                    break;
                case DirectionState.Up:
                    if (isKeyDown(KeyMap.Down)) charDirectionState = DirectionState.Down;
                    if (isKeyIdle(208)) charDirectionState = DirectionState.Forward;        //11010000
                    if (isKeyDown(KeyMap.Backward)) charDirectionState = DirectionState.Backward;
                    break;
                case DirectionState.Down:
                    if (isKeyDown(KeyMap.Up)) charDirectionState = DirectionState.Up;
                    if (isKeyIdle(208)) charDirectionState = DirectionState.Forward;
                    if (isKeyDown(KeyMap.Backward)) charDirectionState = DirectionState.Backward;
                    break;
                case DirectionState.Backward:
                    if (isKeyIdle(208)) charDirectionState = DirectionState.Forward;
                    if (isKeyDown(KeyMap.Up)) charDirectionState = DirectionState.Up;
                    if (isKeyDown(KeyMap.Down)) charDirectionState = DirectionState.Down;
                    break;
            }

            //State Processing

            switch (charMoveState) {
                case MoveState.Stand:
                    if (playerRigidbody2D.velocity.x != 0) {
                        playerRigidbody2D.velocity = new Vector2(0, playerRigidbody2D.velocity.y);
                    }
                    break;
                case MoveState.Walk:
                    if (playerRigidbody2D.velocity.x != horizontalInput * maxWalkSpeed) { 
                        playerRigidbody2D.velocity = new Vector2(horizontalInput * maxWalkSpeed,
                            playerRigidbody2D.velocity.y);
                    }
                    break;
                case MoveState.AirDash:
                    /*
                    playerRigidbody2D.velocity = new Vector2(horizontalInput * maxDashSpeed,
                    playerRigidbody2D.velocity.y);
                    break;
                    */
                case MoveState.AirWalk:
                    if (playerRigidbody2D.velocity.x == 0) {
                        playerRigidbody2D.velocity = new Vector2(horizontalInput * maxWalkSpeed,
                        playerRigidbody2D.velocity.y);
                    }
                    break;
                case MoveState.Dash:
                    if (playerRigidbody2D.velocity.x != horizontalInput * maxDashSpeed) { 
                        playerRigidbody2D.velocity = new Vector2(horizontalInput * maxDashSpeed,
                            playerRigidbody2D.velocity.y);
                    }
                    break;
                case MoveState.Air:
                    if (playerRigidbody2D.velocity.x != 0) { 
                        playerRigidbody2D.velocity = new Vector2(0, playerRigidbody2D.velocity.y);
                    }
                    break;
                case MoveState.Damaged:
                    if (repelTimer == 0) {
                        charJumpState = JumpState.Idle;
                        charFireState = MoveState.Safe;
                        isInvincible = true;
                        playerRigidbody2D.velocity = new Vector2(transform.right.x * repelForce.x, transform.up.y * repelForce.y);

                        playerAnimator.SetTrigger("Damaged");
                    }
                    repelTimer++;
                    if (repelTimer == repelTime) {
                        charMoveState = MoveState.Stand;
                        playerAnimator.SetTrigger("repelFinish");
                        repelTimer = 0;
                    }
                    

                    break;
                case MoveState.Defeated:

                    break;
            }

            switch (charJumpState) {
                case JumpState.Jump:
                    if (jumpTimer < 3) {
                        if (jumpTimer == 0) {
                            playerAnimator.SetTrigger("Jump");
                        }

                        playerRigidbody2D.AddForce(Vector2.up * jumpPower);
                        /*
                        switch (charMoveState) {
                            //case MoveState.Stand:
                                //charMoveState = MoveState.Air;
                                //break;
                            case MoveState.Walk:
                                charMoveState = MoveState.AirWalk;
                                break;
                            case MoveState.Dash:
                                charMoveState = MoveState.AirDash;
                                break;

                        }
                        */
                        jumpTimer++;
                    }
                    break;
                
                case JumpState.Idle:
                    jumpTimer = 0;
                    break;
            }

            switch (charFireState) {
                case MoveState.Safe:
                    playerAnimator.SetBool("Fire", false);
                    if (fireTimer != normalFireDelay) fireTimer = 0;
                    break;
                case MoveState.Fire:
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

            switch (charDirectionState) {
                case DirectionState.Backward:
                    isFacingLeft = !isFacingLeft;
                    transform.rotation = Quaternion.Euler(transform.rotation.x, isFacingLeft ? 180 : 0, transform.rotation.z);
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

        private bool isKeyDown(KeyMap key) {
            return (keyCode & (uint)key) > 0;
        }

        private bool isKeyDown(KeyMap key, int pos) {
            return (keyCodeQueue[MAX_QUEUE_COUNT - pos] & (uint)key) > 0; 
        }

        private bool isKeyIdle(uint mask) {
            return (keyCode & mask) == (uint)KeyMap.Idle;
        }

        private bool isKeyIdle(uint mask, int pos) {
            return (keyCodeQueue[MAX_QUEUE_COUNT - pos] & mask) == (uint)KeyMap.Idle;
        }

        private bool isHoldTimeLessThan(int time, int pos) {
            return keyHoldTimeQueue[MAX_QUEUE_COUNT - pos] <= time;
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
        enum MoveState {
            Stand,
            Walk,
            Crouch,
            CrouchWalk,
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

        enum JumpState {
            Idle,
            Jump,
            Landed
        }

        enum FireState {
            Safe,
            Fire
        }

        enum DirectionState {
            Forward,
            Backward,
            Up,
            Down
        }

        enum DamageState {
            Ok,
            Damaged,
            Muteki,
            Defeated
        }

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