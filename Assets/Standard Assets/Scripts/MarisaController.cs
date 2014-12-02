using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MarisaController : MonoBehaviour {

    private const int MAX_QUEUE_COUNT = 20;

    public float maxWalkSpeed;
    public float maxDashSpeed;
    public float jumpPower;
    public Vector2 repelForce = new Vector2(-4.5f, 5.4f);
    public LayerMask terrainLayer;
    
    private uint keyCode;
    private uint keyHoldTime;
    private List<uint> keyCodeQueue;
    private List<uint> keyHoldTimeQueue;
    
    private State charMoveState;
    private State charJumpState;
    private State charFireState;

    private bool isOnGround;
    private bool isInvincible;
    private bool isFacingLeft;

    private uint jumpTimer;

    private Animator marisaAnimator;
    private BoxCollider2D marisaBoxCollider2D;
    private CircleCollider2D marisaCircleCollider2D;
    private Rigidbody2D marisaRigidbody2D;

    void Reset() {
        Awake();

        maxWalkSpeed = 12f;
        maxDashSpeed = 20f;
        jumpPower = 600f;
        repelForce = new Vector2(-4.5f, 5.4f);
        terrainLayer = 1 << LayerMask.NameToLayer("Terrain");
    }

    void Awake() {
        keyCode = 0;
        keyHoldTime = 0;
        keyCodeQueue = new List<uint>(MAX_QUEUE_COUNT);
        keyHoldTimeQueue = new List<uint>(MAX_QUEUE_COUNT);

        charMoveState = State.Stand;
        charJumpState = State.Stand;
        charFireState = State.Safe;

        isOnGround = false;
        isInvincible = false;
        isFacingLeft = false;

        jumpTimer = 3;

        marisaAnimator = GetComponent<Animator>();
        marisaBoxCollider2D = GetComponent<BoxCollider2D>();
        marisaCircleCollider2D = GetComponent<CircleCollider2D>();
        marisaRigidbody2D = GetComponent<Rigidbody2D>();
    }
    
    void Start() {
        for (int i = 0; i < MAX_QUEUE_COUNT; i++) {      //Initializing queues
            keyCodeQueue.Add(0);
            keyHoldTimeQueue.Add(16);
        }
	}
	
	void Update() {
	    
	}

    void FixedUpdate() {
        GetInput();
        ProcessInput();
        checkIsOnGround();
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
        GUI.Label(new Rect(20, 260, 100, 30), isOnGround.ToString());
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

        //charMoveState

        if ((keyCode & (uint)KeyMap.Forward) > 0) {
                charMoveState = isOnGround ? State.Walk : State.AirWalk;

            if ((keyCodeQueue[MAX_QUEUE_COUNT - 1] == 0) &&             //Dash: Short Forward -> Short Idle -> Forward
                (keyHoldTimeQueue[MAX_QUEUE_COUNT - 1] <= 8)) {
                if (((keyCodeQueue[MAX_QUEUE_COUNT - 2] & (uint)KeyMap.Forward) > 0) && 
                    (keyHoldTimeQueue[MAX_QUEUE_COUNT - 2] <= 8)) {
                    charMoveState = isOnGround ? State.Dash : State.AirDash;
                }
            }
        }
        else if ((keyCode & (uint)KeyMap.Backward) > 0) {
            charMoveState = State.Turning;
        }

        if ((keyCode & 254) == 0)                                       //254 -> 11111110
        {
            charMoveState = isOnGround ? State.Stand : State.Air;

        }




        if ((keyCode & (uint)KeyMap.Jump) > 0) {
            if (isOnGround)
                charJumpState = State.Jump;
        }
        else {
            charJumpState = State.Stand;
        }
        
               


        //charFireState

        if ((keyCode & (uint)KeyMap.Fire) > 0) {
            charFireState = State.Fire;
        }
        else {
            charFireState = State.Safe;
        }

        print(charMoveState);

        //State Processing

        switch (charMoveState) {
            case State.Stand:
                marisaRigidbody2D.velocity = new Vector2(0, marisaRigidbody2D.velocity.y);
                break;
            case State.Walk:
                marisaRigidbody2D.velocity = new Vector2(Input.GetAxis("Horizontal") * maxWalkSpeed,
                    marisaRigidbody2D.velocity.y);
                break;
            case State.AirDash:
                marisaRigidbody2D.velocity = new Vector2(Input.GetAxis("Horizontal") * maxDashSpeed,
                marisaRigidbody2D.velocity.y);
                break;
            case State.AirWalk:
                marisaRigidbody2D.velocity = new Vector2(Input.GetAxis("Horizontal") * maxWalkSpeed, 
                marisaRigidbody2D.velocity.y);
                break;
            case State.Dash:
                marisaRigidbody2D.velocity = new Vector2(Input.GetAxis("Horizontal") * maxDashSpeed, 
                    marisaRigidbody2D.velocity.y);
                break;
            case State.Turning:
                isFacingLeft = !isFacingLeft;
                Quaternion rot = transform.rotation;
                transform.rotation = Quaternion.Euler(rot.x, isFacingLeft ? 180 : 0, rot.z);
                break;
            
            case State.Air:
                marisaRigidbody2D.velocity = new Vector2(0, marisaRigidbody2D.velocity.y);
                break;
        }

        switch (charJumpState) {
            case State.Jump:
                if (jumpTimer > 0) {
                    if (jumpTimer == 3) {
                        marisaAnimator.SetTrigger("Jump");
                        SendMessage("Jump", SendMessageOptions.DontRequireReceiver);
                    }

                    marisaRigidbody2D.AddForce(Vector2.up * jumpPower);
                    switch (charMoveState) {
                        case State.Stand:
                            charMoveState = State.Air;
                            break;
                        case State.Walk:
                            charMoveState = State.AirWalk;
                            break;
                        case State.Dash:
                            charMoveState = State.AirDash;
                            break;

                    }

                    jumpTimer--;
                }
                break;
                
        }

        switch (charFireState) {
            case State.Fire:

                break;
        }

        //Timer reset

        if (charJumpState != State.Jump) {
            jumpTimer = 3;
        }

        //Animation related

        marisaAnimator.SetFloat("Horizontal", marisaRigidbody2D.velocity.x);
        marisaAnimator.SetFloat("Vertical", marisaRigidbody2D.velocity.y);
    }

    void checkIsOnGround() {
        Vector2 pos = transform.position;
        Vector2 leftup = new Vector2(pos.x + marisaCircleCollider2D.center.x - 0.98f * marisaCircleCollider2D.radius,
                                    pos.y + marisaCircleCollider2D.center.y);
        Vector2 rightdown = new Vector2(pos.x + marisaCircleCollider2D.center.x + 0.98f * marisaCircleCollider2D.radius,
                                    pos.y + marisaCircleCollider2D.center.y - 1.1f * marisaCircleCollider2D.radius);

        isOnGround = Physics2D.OverlapArea(leftup, rightdown, terrainLayer);
        marisaAnimator.SetBool("isOnGround", isOnGround);
    }

    IEnumerator yieldTest()
    {
        print("wait2");
        yield return new WaitForSeconds(1);
        print("waited");
    }

    enum State
    {
        Stand,
        Crouch,
        Walk,
        Turning,
        Shoot,
        Dash,
        Air,
        AirWalk,
        AirDash,
        Damage,

        Jump,

        Safe,
        Fire
    };

    enum KeyMap
    {
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
