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
        private FireState charFireState;
        private DirectionState charDirectionState;
        private DamageState charDamageState;

        private CharacterInfo.FireType currentFire;

        private bool isOnGround;
        private bool isFacingLeft;
        private bool isControllable;

        private int fireTimer;
        private int fireCount;
        private int fireDelay;
        private int jumpTimer;
        private int repelTimer;
        private int mutekiTimer;

        private int repelTime;
        private int mutekiTime;

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
        


        public int GetDamaged(int damage) {
            if ((charDamageState != DamageState.Muteki) && (charDamageState != DamageState.Damaged) && (charDamageState != DamageState.Defeated)) {
                HP -= damage;
                if (HP > 0) {
                    charDamageState = DamageState.Damaged;
                }
                else {
                    charDamageState = DamageState.Defeated;
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
                charFireState = FireState.Safe;
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
            mutekiTime = 180;
        }
        
        void Start() {
            charMoveState = MoveState.Stand;
            charJumpState = JumpState.Idle;
            charFireState = FireState.Safe;
            charDirectionState = DirectionState.Forward;
            charDamageState = DamageState.Ok;
            currentFire = CharacterInfo.FireType.Normal;

            isOnGround = false;
            isFacingLeft = false;
            isControllable = false;

            fireTimer = 0;
            fireCount = 0;
            fireDelay = normalFireDelay;
            jumpTimer = 0;
            repelTimer = 0;
            mutekiTimer = 0;

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

            //if ((charMoveState != MoveState.Damaged) && (charMoveState != MoveState.Defeated)) {

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
                        if (playerRigidbody2D.velocity.x == horizontalInput * maxDashSpeed) {
                            charMoveState = MoveState.Dash;
                        }
                        else { 
                            charMoveState = MoveState.Walk;
                        }
                    }
                    break;
                case MoveState.Dash:
                    if (isKeyIdle(48)) {
                        charMoveState = MoveState.Stand;
                    }
                    else if (!isOnGround) {
                        charMoveState = MoveState.AirDash;
                    }
                    break;
                case MoveState.AirDash:
                    if (isKeyIdle(48)) {
                        charMoveState = MoveState.Air;
                    }
                    else if (isOnGround) {
                        charMoveState = MoveState.Dash;
                    }
                    break;
            }

            //charFireState
            switch (charFireState) {
                case FireState.Safe:
                    if (isKeyDown(KeyMap.Fire)) {
                        charFireState = FireState.Fire;
                    }
                    break;
                case FireState.Fire:
                    if (isKeyIdle(2)) {
                        charFireState = FireState.Safe;
                    }
                    break;
            }

            //charJumpState
            switch (charJumpState) {
                case JumpState.Idle:
                    if (isKeyDown(KeyMap.Jump)) {
                        if (isOnGround) {
                            charJumpState = JumpState.Jump;
                        }
                    }
                    break;
                case JumpState.Jump:
                    if (isKeyIdle(65)) {         //01000001
                        charJumpState = JumpState.Idle;
                    }
                    break;
            }

            //charDirectionState
            switch (charDirectionState) {
                case DirectionState.Forward:
                    if (isKeyDown(KeyMap.Backward)) charDirectionState = DirectionState.Backward;
                    else if (isKeyDown(KeyMap.Down)) charDirectionState = DirectionState.Down;
                    else if (isKeyDown(KeyMap.Up)) charDirectionState = DirectionState.Up;
                    break;
                case DirectionState.Up:
                    if (isKeyDown(KeyMap.Backward)) charDirectionState = DirectionState.Backward;
                    else if (isKeyIdle(208)) charDirectionState = DirectionState.Forward;        //11010000
                    else if (isKeyDown(KeyMap.Down)) charDirectionState = DirectionState.Down;
                    break;
                case DirectionState.Down:
                    if (isKeyDown(KeyMap.Backward)) charDirectionState = DirectionState.Backward;
                    else if (isKeyIdle(208)) charDirectionState = DirectionState.Forward;
                    else if (isKeyDown(KeyMap.Up)) charDirectionState = DirectionState.Up;
                    break;
                case DirectionState.Backward:
                    if (isKeyDown(KeyMap.Down)) charDirectionState = DirectionState.Down;
                    else if (isKeyDown(KeyMap.Up)) charDirectionState = DirectionState.Up;
                    else if (isKeyIdle(208)) charDirectionState = DirectionState.Forward;
                    break;
            }

            //State Processing
            if ((charDamageState != DamageState.Damaged) && (charDamageState != DamageState.Defeated)) {
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
                    case MoveState.AirWalk:
                        if (playerRigidbody2D.velocity.x != horizontalInput * maxWalkSpeed) {
                            playerRigidbody2D.velocity = new Vector2(horizontalInput * maxWalkSpeed,
                            playerRigidbody2D.velocity.y);
                        }
                        break;
                    case MoveState.AirDash:
                        if (playerRigidbody2D.velocity.x != horizontalInput * maxDashSpeed) {
                            playerRigidbody2D.velocity = new Vector2(horizontalInput * maxDashSpeed,
                            playerRigidbody2D.velocity.y);
                        }
                        break;
                }

                switch (charJumpState) {
                    case JumpState.Jump:
                        if (jumpTimer < 3) {
                            if (jumpTimer == 0) {
                                playerAnimator.SetTrigger("Jump");
                            }
                            playerRigidbody2D.AddForce(Vector2.up * jumpPower);
                            jumpTimer++;
                        }
                        break;

                    case JumpState.Idle:
                        jumpTimer = 0;
                        break;
                }

                switch (charFireState) {
                    case FireState.Safe:
                        playerAnimator.SetBool("Fire", false);
                        if (fireTimer != normalFireDelay) fireTimer = 0;
                        break;
                    case FireState.Fire:
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

            }

            switch (charDamageState) {
                case DamageState.Damaged:
                    if (repelTimer == 0) {
                        charMoveState = MoveState.Stand;
                        charJumpState = JumpState.Idle;
                        charFireState = FireState.Safe;
                        playerRigidbody2D.velocity = new Vector2(transform.right.x * repelForce.x, transform.up.y * repelForce.y);

                        playerAnimator.SetTrigger("Damaged");
                    }
                    repelTimer++;
                    if (repelTimer == repelTime) {
                        charDamageState = DamageState.Muteki;
                        playerAnimator.SetTrigger("repelFinish");
                        repelTimer = 0;
                    }
                    break;
                case DamageState.Defeated:

                    break;
                case DamageState.Muteki:
                    if (mutekiTimer % 2 == 0) {
                        playerSpriteRenderer.color = new Color(1f, 1f, 1f, 0f);
                    }
                    else {
                        playerSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                    }
                    mutekiTimer++;
                    if (mutekiTimer == mutekiTime) {
                        playerSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
                        charDamageState = DamageState.Ok;
                        mutekiTimer = 0;
                    }
                    break;
            }

            //Speed Limitation
            if (playerRigidbody2D.velocity.y > 20f) {
                playerRigidbody2D.velocity = new Vector2(playerRigidbody2D.velocity.x, 20f);
            }
            if (playerRigidbody2D.velocity.y < -20f) {
                playerRigidbody2D.velocity = new Vector2(playerRigidbody2D.velocity.x, -20f);
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

        private void CheckIsOnGround() {
            Vector2 pos = transform.position;
            Vector2 leftup = new Vector2(pos.x + playerCircleCollider2D.center.x - 0.98f * playerCircleCollider2D.radius,
                                        pos.y + playerCircleCollider2D.center.y);
            Vector2 rightdown = new Vector2(pos.x + playerCircleCollider2D.center.x + 0.98f * playerCircleCollider2D.radius,
                                        pos.y + playerCircleCollider2D.center.y - 1.1f * playerCircleCollider2D.radius);

            isOnGround = Physics2D.OverlapArea(leftup, rightdown, terrainLayer);
            playerAnimator.SetBool("isOnGround", isOnGround);
        }

        private void GenerateBullet(CharacterInfo.FireType type) {
            Quaternion rotation = Quaternion.Euler(0, 0, 0);
            if (charDirectionState == DirectionState.Up) {
                rotation = Quaternion.Euler(0, 0, 90.0f);
            }
            else if (charDirectionState == DirectionState.Down && (!isOnGround)) {
                rotation = Quaternion.Euler(0, 0, 270.0f);
            }
            else if (isFacingLeft) {
                rotation = Quaternion.Euler(0, 0, 180.0f);
            }
            

            switch (type) {
                case CharacterInfo.FireType.Normal:
                    Rigidbody2D bulletClone = (Rigidbody2D)Instantiate(normalFire, transform.position, transform.rotation);
                    bulletClone.GetComponent<Bullet>().setDirection(rotation);
                    //bulletClone.velocity = new Vector2(isFacingLeft ? -1 : 1, 0) * normalFire.GetComponent<Bullet>().speed;
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
        private enum MoveState {
            Stand,
            Walk,
            Crouch,
            CrouchWalk,
            Dash,
            Air,
            AirWalk,
            AirDash,
        };

        private enum JumpState {
            Idle,
            Jump
        }

        private enum FireState {
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