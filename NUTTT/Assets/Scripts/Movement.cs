using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Movement : MonoBehaviour
{
    private Collision coll;
    private Rigidbody2D rb;

    [Header("Stats")]
    public float speed = 10f;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp =10 ;
    public float dashSpeed = 20;

    [Header("Check")]

    public float coyoteTime;
    public float lastOnGroundTime;
    [Space]
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;

    private bool facingRight;

    private bool hasDashed;
    private bool isGrounded;

    public int side = 1;

    // Start is called before the first frame update
    void Start()
    {
        facingRight = true;

        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collision>();
    }

    // Update is called once per frame
    void Update()
    {
        lastOnGroundTime -= Time.deltaTime;


        //Get x and y axis
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        Walk(dir);

        //Start WallGrab if hold Shift key
        if(coll.onWall && Input.GetButton("Fire3") && canMove)
        {
            //Make player face against the wall
            if (side != coll.wallSide)
                Flip(side * -1);

            wallGrab = true;
            wallSlide = false;
        }

        //Stop wallGrab or wallSlide
        if(Input.GetButtonUp("Fire3") || !coll.onWall || !canMove)
        {
            wallGrab = false;
            wallSlide = false;
        }

        //Enable BetterJump 
        if(coll.onGround && !isDashing)
        {
            wallJumped = false;
            GetComponent<BetterJump>().enabled = true;
        }
        
        //Disable gravity if wallGrab when not dashing && FACKING WALL CLIMBB
        if(wallGrab && !isDashing)
        {

            //Disable gravity if wallGrab when !dashing
            rb.gravityScale = 0;

            //Return normal velocity if moveInput > 0
            if(x > 0.2f || x < -0.2f)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }

            //THIS IS FACKING WALL CLIMBB
            float speedModifier = y > 0 ? 0.5f : 1;

            rb.velocity = new Vector2(rb.velocity.x, y * (speed * speedModifier));
        }
        //Reset gravity to normal
        else
        {
            rb.gravityScale = 3;
        }

        //Start wallSlide if onWall && !wallGrab-ing && when press either A or D
        if(coll.onWall && !coll.onGround)
        {
            if(x != 0 && !wallGrab)
            {
                wallSlide = true;
                WallSlide();
            }
        }

        //Disable wallSlide if onGround || not onWall
        if(!coll.onWall || coll.onGround)
        {
            wallSlide = false;
        }

        //Make jump if onGround and wallJump if onWall
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (lastOnGroundTime > 0)
                Jump(Vector2.up, false);
            if(coll.onWall && !coll.onGround)
                WallJump();
        }

        //Make player dash to the direction we are moving
        if (Input.GetKeyDown(KeyCode.F) && !hasDashed)
        {
            if(xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        //Reset dash and you are grounded
        if(coll.onGround && !isGrounded)
        {
            GroundTouch();
            isGrounded = true;
        }

        //Reset time on ground
        if (coll.onGround && isGrounded)
            lastOnGroundTime = coyoteTime;

        //You're not grounded
        if (!coll.onGround && isGrounded)
            isGrounded = false;

        //Nothing happen??
        if (wallGrab || wallSlide || !canMove)
            return;

        //Flip player???
        if (x > 0)
        {
            side = 1;
            Flip(side);
        }
        if(x < 0)
        {
            side = -1;
            Flip(side);
        }

    }

    void GroundTouch()
    {
        //Reset dash
        hasDashed = false;
        isDashing = false;

        //Detect side
        if (Input.GetAxisRaw("Horizontal") > 0)
            side = 1;
        else if (Input.GetAxisRaw("Horizontal") < 0)
            side = -1;
    }

    private void Dash(float x, float y)
    {
        hasDashed = true;

        //Reset Velocity and Give New Dash Velocity
        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        //Dash
        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        StartCoroutine(GroundDash());

        rb.gravityScale = 0;
        GetComponent<BetterJump>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(0.3f);

        rb.gravityScale = 3;
        GetComponent <BetterJump>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    //Reset Dash if isGrounded
    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(0.15f);
        if (coll.onGround)
            hasDashed = false;
    }

    private void WallJump()
    {
        if((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            //Still facking flip player
            side *= -1;
            Flip(side);

            //Make player can move instanly
            StopCoroutine(DisableMovement(0f));

            //Make player move after .1 sec????
            //WTFFF
            StartCoroutine(DisableMovement(0.1f));

            //Decide which way to wallJump
            Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

            //
            Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

            wallJumped = true;
        }
    }


    private void WallSlide()
    {
        //Make player face out of the wall while sliding
        if (coll.wallSide != side)
            Flip(side * -1);

        if (!canMove)
            return;

        bool pushingWall = false;

        //Make pushingWall true if running straight into the wall while on the wall(i don't know what da fack that mean)
        if((coll.onRightWall && rb.velocity.x > 0) || (coll.onLeftWall && rb.velocity.x < 0))
        {
            pushingWall = true;
        }

        //Choose value for wall( make us stick to wall) if pressing move button or do nothing 
        float push = pushingWall ? 0 : rb.velocity.x;

        //Make player slide with the speed of slideSpeed and stick to the wall if press A or D to the wall
        rb.velocity = new Vector2(push, -slideSpeed);
    }


    private void Walk(Vector2 dir)
    {
        //Disable Walk if !canMove and when wallGrab
        if (!canMove)
            return;
        if (wallGrab)
            return;

        //Make normal movement
        if (!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        }
        //Increase speed to normal after wallJump
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }


    private void Jump(Vector2 dir, bool wall)
    {
        //Reset velocity
        rb.velocity = new Vector2(rb.velocity.x, 0);
        //Make Jump
        rb.velocity += dir * jumpForce;
    }


    //Disable Movement in a period of float 'time'
    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    //Adjust playerDrag
    private void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    //Flip player
    private void Flip(int side)
    {
        Vector3 Scaler = transform.localScale;
        Scaler.x = side;
        transform.localScale = Scaler;
        facingRight = !facingRight;
    }
}
