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
    private uint timer;
    private List<uint> keyCodeQueue;
    private List<uint> keyHoldTimeQueue;
    private State charState;
    private bool isOnGround;
    private bool isInvincible;
    private bool isFacingLeft;

    private Animator marisaAnimator;
    private BoxCollider2D marisaBoxCollider2D;
    private CircleCollider2D marisaCircleCollider2D;
    private Rigidbody2D marisaRigidbody2D;

    void Reset() {
        Awake();

        maxWalkSpeed = 12f;
        maxDashSpeed = 20f;
        jumpPower = 10f;
        repelForce = new Vector2(-4.5f, 5.4f);
        terrainLayer = 1 << LayerMask.NameToLayer("Terrain");
    }

    void Awake() {
        keyCode = 0;
        keyHoldTime = 0;
        keyCodeQueue = new List<uint>(MAX_QUEUE_COUNT);
        keyHoldTimeQueue = new List<uint>(MAX_QUEUE_COUNT);
        charState = State.Stand;
        isOnGround = false;
        isInvincible = false;
        isFacingLeft = false;

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

        GUI.Label(new Rect(20, 215, 100, 30), charState.ToString());
        GUI.Label(new Rect(20, 230, 100, 30), isOnGround.ToString());
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

        //keyCode -> State

        if ((keyCode & (uint)KeyMap.Forward) > 0) {
            if ((keyCode & (uint)KeyMap.Fire) > 0) {
                charState = isOnGround ?                                //WalkShoot: Forward & Shoot
                    State.WalkShoot : State.AirWalkShoot;               //AirWalkShoot : WalkShoot in air
            }
            else {
                charState = isOnGround ? State.Walk : State.AirWalk;
            }

            if ((keyCodeQueue[MAX_QUEUE_COUNT - 1] == 0) &&             //Dash: Short Forward -> Short Idle -> Forward
                (keyHoldTimeQueue[MAX_QUEUE_COUNT - 1] <= 8)) {
                if (((keyCodeQueue[MAX_QUEUE_COUNT - 2] & (uint)KeyMap.Forward) > 0) && 
                    (keyHoldTimeQueue[MAX_QUEUE_COUNT - 2] <= 8)) {
                    charState = isOnGround ? State.Dash : State.AirDash;
                }
            }
        }
        else if ((keyCode & (uint)KeyMap.Backward) > 0) {
            charState = State.Turning;
        }

        if ((keyCode & 254) == 0)
        {
            charState = isOnGround ? State.Stand : State.Air;
        }

        if ((keyCode & (uint)KeyMap.Jump) > 0)
        {
            if (isOnGround)
                charState = State.Jump;
        }

        
        

        //State Processing

        switch (charState) {
            case State.Stand:
                marisaRigidbody2D.velocity = new Vector2(0, marisaRigidbody2D.velocity.y);
                break;
            case State.WalkShoot:

            case State.Walk:
            case State.AirWalk:
                marisaRigidbody2D.velocity = new Vector2(Input.GetAxis("Horizontal") * maxWalkSpeed, 
                    marisaRigidbody2D.velocity.y);
                break;
            case State.Dash:
                marisaRigidbody2D.velocity = new Vector2(Input.GetAxis("Horizontal") * maxDashSpeed, 
                    marisaRigidbody2D.velocity.y);
                break;
            case State.AirDash:
               
                break;
            case State.Turning:
                isFacingLeft = !isFacingLeft;
                Quaternion rot = transform.rotation;
                transform.rotation = Quaternion.Euler(rot.x, isFacingLeft ? 180 : 0, rot.z);
                break;
            case State.Jump:
                //isOnGround = false;
                marisaRigidbody2D.AddForce(Vector2.up * jumpPower);
                charState = State.Air;
                break;
            case State.Air:
                marisaRigidbody2D.velocity = new Vector2(0, marisaRigidbody2D.velocity.y);
                break;
        }
    }

    void checkIsOnGround() {
        Vector2 pos = transform.position;
        Vector2 leftup = new Vector2(pos.x + marisaCircleCollider2D.center.x - 0.2f * marisaCircleCollider2D.radius,
                                    pos.y + marisaCircleCollider2D.center.y);
        Vector2 rightdown = new Vector2(pos.x + marisaCircleCollider2D.center.x + 0.2f * marisaCircleCollider2D.radius,
                                    pos.y + marisaCircleCollider2D.center.y - marisaCircleCollider2D.radius);
        isOnGround = Physics2D.OverlapArea(leftup, rightdown, terrainLayer);
    }

    enum State
    {
        Stand,
        Crouch,
        Walk,
        Turning,
        Shoot,
        WalkShoot,
        Dash,
        Jump,
        Air,
        AirWalk,
        AirWalkShoot,
        AirDash,
        Damage
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
