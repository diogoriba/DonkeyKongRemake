using UnityEngine;
using System.Collections;

public class CharacterMovement : MonoBehaviour 
{
    public Transform cameraPivot;
    public float moveSpeed = 5f;
    public float turnSpeed = 8f;
    public float jumpForce = 5f;
    public Ladder ladder;
    public bool ladderStartedClimb;
    public float groundCollisionExtent = 0.5f;
    public bool grounded;
    public bool jumped;
    public bool dead;
    public bool won;
    int floorMask;
    Vector3 lastMove;
    Animator anim;

    public void EnterLadder(Ladder ladder)
    {
        this.ladder = ladder;
        rigidbody.useGravity = false;
        rigidbody.velocity = new Vector3(0, 0, 0);
        ladderStartedClimb = false;
    }

    public void LeaveLadder()
    {
        rigidbody.useGravity = true;
        if (!grounded)
        {
            Vector3 impulseForward = transform.forward * ladder.impulseForwardForce;
            Vector3 impulseUp = Vector3.up * ladder.impulseUpForce;
            rigidbody.AddForce(impulseForward + impulseUp, ForceMode.Impulse); // unlatching from the top
        }
        ladder = null;
    }
       
    // Use this for initialization
	void Awake () {
        cameraPivot = GameObject.Find("CameraPivot").transform;
        floorMask = LayerMask.GetMask("Floor");
        grounded = true;
        jumped = false;
        dead = false;
        won = false;
        ladderStartedClimb = false;
        lastMove = transform.forward;
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (!dead && !won)
        {
            if (networkView.isMine || (!Network.isServer && !Network.isClient))
            {
                UpdateGrounded();
                Move();
                Rotate();
                Jump();
                CheckKillPlane();                            
            }           
        }
	}

    private void CheckKillPlane()
    {
        if (transform.position.y < -10)
        {
            Die();
        }
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        //Animating(h, v);
        
        Vector3 moveDirection = transform.forward;
        if (ladder)
        {
            // let ladder control the user ?
            // how to make it so that we can control it with left/right depending on where the ladder is relative to the camera/player?
            moveDirection = v * Vector3.up;
            moveDirection *= moveSpeed;
            moveDirection *= Time.deltaTime;
            rigidbody.position += moveDirection;

            if (!ladderStartedClimb && !grounded)
            {
                ladderStartedClimb = true;
            }
            else if (ladderStartedClimb && grounded)
            {
                LeaveLadder();
            }
        }
        else if (grounded)
        {
            moveDirection = v * cameraPivot.forward;
            moveDirection += h * cameraPivot.right;
            moveDirection *= moveSpeed;
            moveDirection *= Time.deltaTime;
            lastMove = moveDirection;
            rigidbody.position += moveDirection;

            Animating(h, v);            
        }
        else if (jumped)
        {
            rigidbody.position += lastMove;
        }
    }

    private void Rotate()
    {
        Quaternion lookRotation = transform.rotation;

        if (!ladder)
        {
            Vector3 orientation = lastMove;
            if (orientation.magnitude == 0f)
            {
                orientation = transform.forward;
            }
            lookRotation = Quaternion.LookRotation(orientation, transform.up);
        }
        else
        {
            Vector3 playerToLadder = ladder.transform.position - transform.position;
            playerToLadder.y = 0;
            lookRotation = Quaternion.LookRotation(playerToLadder, transform.up);
        }

        lookRotation.x = 0;
        lookRotation.z = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime); // we could use raycasting to look perpendicular to the object, but nah
    }

    private void UpdateGrounded()
    {
        RaycastHit hit;
        grounded = Physics.SphereCast(transform.position, 0.5f, -Vector3.up, out hit, groundCollisionExtent, floorMask);
        if (jumped)
        {
            jumped = !grounded;
        }
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && grounded && ladder == null)
        {
            rigidbody.AddForce(jumpForce * Vector3.up);
            jumped = true;
        }
    }

    public void Die()
    {
        if (networkView.isMine || Network.peerType == NetworkPeerType.Disconnected)
        {
            dead = true;
            GameOverManager.instance.GameOver();
        }
    }

    public void Win()
    {
        if (networkView.isMine || Network.peerType == NetworkPeerType.Disconnected)
        {
            won = true;
            VictoryManager.instance.Win();
        }
    }

    void Animating (float h, float v)
    {
        bool walking = h != 0f || v != 0f;

        anim.SetBool("IsWalking", walking); 
    }
}
