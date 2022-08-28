using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PM2 : MonoBehaviour
{
    // Declare game object's components to manipulate

    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public Animator anim;
    public GameObject shotPoint;
    public GameObject bullet;

    // Keeps track of if the game sprite is actually facing right

    private bool facingRight = true;

    // Variables related to checking if the player is grounded
    public bool isGrounded = false;
    public int groundCounter;

    // Bools tracking if the player is currently doing something
    public bool isShooting = false;
    public bool isSliding = false;

    // Used to check if we've stopped doing something last frame
    public bool wasSlidingLastFrame = false;
    public bool wasGroundedLastFrame = false;

    // Unused bool to possibly limit the slide anim playing when jumping very close to ground into walls 
    public bool canSlide = true;

    // Has a button been pressed
    public bool jump = false;
    public bool fireButton = false;

    // Move now essentially tracks if the player should move laterally or not
    public int movement = 0;

    // Variables for dashing
    public float dashSpeed = 1;
    public bool canDash = true;

    // Left-right axis
    public float horizontal = 0f;

    // Used to target Main Tilemap Layer for raycasts
    int slideMask;

    // Passed to animator for y axis up/down jump animations
    public Vector3 playerVelocity = new Vector2(0, 0);
    //public Vector3 vec = new Vector3(0, 0, 0);

    // Variable that gets set and then determines wall jump vector
    public Vector2 slideJumpVec = new Vector2(0, 1);
    public bool isWallJumping = false;

    // Jump and double jump variables
    public bool isJumping = false;
    public bool canDoubleJump = false;

    // Groundcast normals to use when determining slope movement
    private Vector2 dirRightGroundNormal = new Vector2(0, 0);
    private Vector2 dirLeftGroundNormal = new Vector2(0, 0);
    private Vector2 moveNormal = new Vector2(0, 0);
    private int normDirFactor;

    // Variables to hold the raycast hits for grounding, walls, and sliding
    List<RaycastHit2D> lateralHits;
    List<RaycastHit2D> groundHits;
    RaycastHit2D rightTopHit;
    RaycastHit2D rightMiddleHit;
    RaycastHit2D rightBottomHit;
    RaycastHit2D leftTopHit;
    RaycastHit2D leftMiddleHit;
    RaycastHit2D leftBottomHit;

    // Sets the animation parameters
    void SetAnimParams()
    {
        anim.SetFloat("Speed", Mathf.Abs(horizontal));
        anim.SetFloat("VerticalVelocity", playerVelocity.y);
        anim.SetBool("isShooting", isShooting);
        anim.SetBool("isSliding", isSliding);

        if (isGrounded)
        {
            anim.SetBool("isGrounded", true);
        }
        else
        {
            anim.SetBool("isGrounded", false);
        }
    }

    bool IsFacingRight(bool isRight)
    {
        return isRight;
    }

    bool ShouldFlipSprite(bool isRight)
    {
        return !isRight;
    }

    int LateralMovementCheck(bool isRight)
    {
        return isRight ? 1 : -1;
    }

    List<RaycastHit2D> LateralRaycastCheck(bool isRight)
    {
        List<RaycastHit2D> lateralHitsToUse = new List<RaycastHit2D>();

        if(isRight)
        {
            lateralHitsToUse.Add(rightTopHit);
            lateralHitsToUse.Add(rightMiddleHit);
            lateralHitsToUse.Add(rightBottomHit);
        }

        if(!isRight)
        {
            lateralHitsToUse.Add(leftTopHit);
            lateralHitsToUse.Add(leftMiddleHit);
            lateralHitsToUse.Add(leftBottomHit);
        }

        return lateralHitsToUse;
    }

    void AdjustShotPoint(bool isRight)
    {
        float shotpointOffset = isRight ? -.6f : .6f;

        if (isSliding)
        {
            shotPoint.transform.position = new Vector3(rb.position.x + shotpointOffset, rb.position.y, 0);
        }
        else
        {
            shotPoint.transform.position = new Vector3(rb.position.x + -shotpointOffset, rb.position.y, 0);
        }
    }

    Vector2 DetermineWallJumpVec(bool isRight)
    {
        return isRight ? new Vector2(-2, 4) : new Vector2(2, 4);
    }

    //Vector2 DetermineGroundNormal(bool isRight)
    //{
    //    return isRight ? dirRightGroundNormal : dirLeftGroundNormal;
    //}

    int DetermineNormDirFactor(bool isRight)
    {
        return isRight ? 1 : -1;
    }

    // This currently gets run when either horizontal input is pressed and is used to
    // determine different values that we need to change when the character changes direction
    void InterpretHorizontalInput(bool isRight)
    {

        int thisMovement = LateralMovementCheck(isRight);

        List<RaycastHit2D> lateralHits = LateralRaycastCheck(isRight);
        RaycastHit2D lateralTopHit = lateralHits[0];
        RaycastHit2D lateralMidHit = lateralHits[1];
        RaycastHit2D lateralBottomHit = lateralHits[2];

        // Omit bottom lateral raycasts to enable slope walking for now

        if (lateralMidHit || lateralTopHit)
        {
            thisMovement = 0;
        }

        // Set the class variables to the left or right ones according to input
        movement = thisMovement;
        facingRight = IsFacingRight(isRight);
        sprite.flipX = ShouldFlipSprite(isRight);
        slideJumpVec = DetermineWallJumpVec(isRight);
        //moveNormal = DetermineGroundNormal(isRight);

        normDirFactor = DetermineNormDirFactor(isRight);

        AdjustShotPoint(isRight);

        if (lateralMidHit.collider == true && !isGrounded)
        {
            isSliding = true;
            canDash = false;

            if(!isWallJumping)
            {
                canDoubleJump = false;
            }
        }
        else
        {
            isSliding = false;
        }
    }

    // Determine direction of the bullet, instantiate it and set the is shooting timer coroutine
    void Shoot()
    {
        GameObject newBullet;
        Vector2 forward = facingRight ? -Vector2.left : Vector2.left;
        isShooting = true;
        StopCoroutine("SetIsShootingToFalse");
        StartCoroutine("SetIsShootingToFalse");
        newBullet = Instantiate(bullet, shotPoint.transform.position, Quaternion.identity);
        if (isSliding)
        {
            newBullet.GetComponent<BaseShotScript>().direction = -forward * 10;
        }
        else
        {
            newBullet.GetComponent<BaseShotScript>().direction = forward * 10;
        }
    }

    // Contains all the downard groundcasts used to determine grounding and slope normals
    // The lengths are carefuly crafted so far to match the Physics2D settings and
    // balance grounding on edges and the measurement of slopes
    private List<RaycastHit2D> GroundedCasts()
    {
        // Lengths use to be .01f, but increased to .06f for slope intersection detection since we extend it up through collider .05f

        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        // The two outer casts have a .15f indent to match the player collider
        // All three start at -.45f to start inside the collider and interesect with slopes to read normals
        // -.08f length is maximum we'd want to set here and represents the length of the ray.
        // Any more and we'll risk being grounded when we shouldn't be

        Debug.DrawRay(transform.position + new Vector3(-.15f, -.45f, 0), new Vector2(0, -.06f));
        RaycastHit2D bottomLeft = Physics2D.Raycast(transform.position + new Vector3(-.15f, -.45f, 0), -Vector2.up, .06f, slideMask);
        hits.Add(bottomLeft);

        Debug.DrawRay(transform.position + new Vector3(0, -.45f, 0), new Vector2(0, -.06f));
        RaycastHit2D bottomMid = Physics2D.Raycast(transform.position + new Vector3(0, -.45f, 0), -Vector2.up, .06f, slideMask);
        hits.Add(bottomMid);

        Debug.DrawRay(transform.position + new Vector3(.15f, -.45f, 0), new Vector2(0, -.06f));
        RaycastHit2D bottomRight = Physics2D.Raycast(transform.position + new Vector3(.15f, -.45f, 0), -Vector2.up, .06f, slideMask);
        hits.Add(bottomRight);

        return hits;
    }

    // Three lateral casts per side, top, middle, and bottom. The bottom two casts are
    // currently located as though the player collider is a square - this could change
    // If either of the four uppermost casts touch a tilemap collider, we will set movement
    // variable to zero. Bottom two are omitted to allow slopes
    // Top is shortened .01f to account for Physicys2D collision extrusion
    private List<RaycastHit2D> LateralCasts()
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        RaycastHit2D rightTop = Physics2D.Raycast(transform.position + new Vector3(.25f, .49f, 0), -Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + new Vector3(.25f, .49f, 0), new Vector2(.01f, 0));
        hits.Add(rightTop);

        RaycastHit2D rightMid = Physics2D.Raycast(transform.position + new Vector3(.25f, 0, 0), -Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + new Vector3(.25f, 0, 0), new Vector2(.01f, 0));
        hits.Add(rightMid);

        RaycastHit2D rightBottom = Physics2D.Raycast(transform.position + new Vector3(.25f, -.5f, 0), -Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + new Vector3(.25f, -.5f, 0), new Vector2(.01f, 0));
        hits.Add(rightBottom);

        RaycastHit2D leftTop = Physics2D.Raycast(transform.position + new Vector3(-.25f, .49f, 0), Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + new Vector3(-.25f, .49f, 0), new Vector2(-.01f, 0));
        hits.Add(leftTop);

        RaycastHit2D leftMid = Physics2D.Raycast(transform.position + new Vector3(-.25f, 0, 0), Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + new Vector3(-.25f, 0, 0), new Vector2(-.01f, 0));
        hits.Add(leftMid);

        RaycastHit2D leftBottom = Physics2D.Raycast(transform.position + new Vector3(-.25f, -.5f, 0), Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + new Vector3(-.25f, -.5f, 0), new Vector2(-.01f, 0));
        hits.Add(leftBottom);

        return hits;
    }

    // Counter to set shooting back to false
    // Only for animation right now but could limit shots with a can shoot bool
    private IEnumerator SetIsShootingToFalse()
    {
        yield return new WaitForSeconds(1f);
        if (isShooting)
        {
            isShooting = false;
        }
    }

    // Unused but sets can slide to true on a counter
    private IEnumerator CanSlideToTrue()
    {
        yield return new WaitForSeconds(.1f);
        if (!canSlide)
        {
            canSlide = true;
        }
    }

    // Jump routine using force instead of translate
    private IEnumerator JumpRoutine()
    {
        isJumping = true;
        canDoubleJump = true;

        rb.velocity = Vector2.zero;
        Vector2 jumpVector = rb.position;
        float jumpTime = .2f;
        float timer = 0;

        while (Input.GetButton("Jump") && timer < jumpTime)
        {
            yield return new WaitForFixedUpdate();
            float proportionCompleted = timer / jumpTime;
            Vector2 thisFrameJumpVector = Vector2.Lerp(Vector2.up, Vector2.zero, proportionCompleted);
            rb.AddForce(thisFrameJumpVector, ForceMode2D.Impulse);
            //transform.Translate(thisFrameJumpVector/10);
            timer += Time.deltaTime;
            yield return null;
        }

        isJumping = false;
    }

    // Not sure how to use arguments with coroutines so make a double
    private IEnumerator DoubleJumpRoutine()
    {
        canDoubleJump = false;

        rb.velocity = Vector2.zero;
        Vector2 jumpVector = rb.position;
        float jumpTime = .2f;
        float timer = 0;

        while (Input.GetButton("Jump") && timer < jumpTime)
        {
            yield return new WaitForFixedUpdate();
            float proportionCompleted = timer / jumpTime;
            Vector2 thisFrameJumpVector = Vector2.Lerp(Vector2.up, Vector2.zero, proportionCompleted);
            rb.AddForce(thisFrameJumpVector, ForceMode2D.Impulse);
            //transform.Translate(thisFrameJumpVector/10);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    // Unused copy of jump meant to become the coroutine for wall jumping
    private IEnumerator Walljump()
    {
        canDash = false;
        canDoubleJump = true;
        isWallJumping = true;

        rb.velocity = Vector2.zero;
        Vector2 jumpVector = slideJumpVec;
        float jumpTime = .1f;
        float timer = 0;
        bool isRight = jumpVector.x > 0 ? true : false;
        bool isPressingTowardsWall = isRight ? horizontal > 0 : horizontal < 0;
        
        while (Input.GetButton("Jump") && timer < jumpTime)
        {
            yield return new WaitForFixedUpdate();
            float proportionCompleted = timer / jumpTime;
            Vector2 thisFrameJumpVector = Vector2.Lerp(Vector2.zero, jumpVector / 2, proportionCompleted);
            //rb.AddForce(new Vector2(0, thisFrameJumpVector.y) * Time.deltaTime * 10, ForceMode2D.Impulse);
            rb.AddForce(new Vector2(0, thisFrameJumpVector.y), ForceMode2D.Impulse);
            transform.Translate(new Vector2(thisFrameJumpVector.x, 0)/5);

            timer += Time.deltaTime;
            yield return null;
        }

        isWallJumping = false;
        canDash = true;
    }

    private IEnumerator Dash()
    {
        dashSpeed = 3;
        float dashTime = .2f;
        float timer = 0;

        while (Input.GetButton("Fire3") && timer < dashTime)
        {
            //wait
            timer += Time.deltaTime;
            yield return null;
        }

        dashSpeed = 1;
        yield return null;

    }

    // Start is called before the first frame update
    void Start()
    {
        // Set initial variables

        rb = gameObject.GetComponent<Rigidbody2D>();
        sprite = gameObject.GetComponent<SpriteRenderer>();
        anim = gameObject.GetComponent<Animator>();

        slideMask = LayerMask.GetMask("Main Tilemap Layer");
    }

    // Update is called once per frame
    void Update()
    {
        // Set movement to 0 every frame so if there is no input we don't move
        movement = 0;

        // Get the inputs we need
        horizontal = Input.GetAxisRaw("Horizontal");
        jump = Input.GetButtonDown("Jump");
        fireButton = Input.GetButtonDown("Fire1");

        // Get anim variables from rigidbody
        playerVelocity = rb.velocity;

        //SetAnimParams();

        // Grounding raycast
        groundHits = GroundedCasts();

        // Lateral raycasts
        lateralHits = LateralCasts();
        rightTopHit = lateralHits[0];
        rightMiddleHit = lateralHits[1];
        rightBottomHit = lateralHits[2];
        leftTopHit = lateralHits[3];
        leftMiddleHit = lateralHits[4];
        leftBottomHit = lateralHits[5];

        // Delay being grounded by a few frames to smooth out edges

        bool addOne = false;

        foreach (RaycastHit2D hit in groundHits)
        {
            if (hit.collider == true)
            {
                addOne = true;
                break;
            }
        }

        // Counter to delay the setting of grounding

        if (addOne && !isGrounded)
        {
            groundCounter++;
        }
        else if (!addOne && isGrounded)
        {
            isGrounded = false;
        }

        // Arbitray counter value that just seemed to work well, used to be 8
        if (groundCounter >= 1)
        {
            groundCounter = 0;
            isGrounded = true;

            if(!isJumping)
            {
                canDoubleJump = false;
            }

        }

        // Check and set slope normals

        // Right cast
        if (groundHits[2])
        {
            RaycastHit2D hit = groundHits[2];
            moveNormal = new Vector2(hit.normal.y, -hit.normal.x);
        }

        // Left cast
        if (groundHits[0])
        {
            RaycastHit2D hit = groundHits[0];
            moveNormal = new Vector2(hit.normal.y, -hit.normal.x);
        }

        if(!isGrounded)
        {
            moveNormal = new Vector2(1, 0);
        }

        // Drag to make you slower aong walls when wall sliding
        if (isSliding)
        {
            rb.drag = 20f;
        }
        else
        {
            rb.drag = 0f;
        }

        // Shoot the bullet if it's pressed
        if (fireButton)
        {
            Shoot();
        }

        // Run Interpret only if there is a directional input
        // May need to run it with a no input version to reset variables properly
        if (horizontal > .1f)
        {
            InterpretHorizontalInput(true);
        }
        else if (horizontal < -.1f)
        {
            InterpretHorizontalInput(false);
        }
        else
        {
            isSliding = false;
        }

        // If the character is currently grounded and jump button is pressed
        // Then...jump
        if (jump && isGrounded)
        {
            StartCoroutine("JumpRoutine");
        }

        if (jump && canDoubleJump && !isJumping)
        {
            StopCoroutine("JumpRoutine");
            StartCoroutine("DoubleJumpRoutine");
        }

        // You get it - this is where wall jump coroutine will go
        if (jump && isSliding)
        {
            //rb.AddForce(slideJumpVec, ForceMode2D.Impulse);
            StopCoroutine("Walljump");
            StartCoroutine("Walljump");
        }

        if(Input.GetButtonDown("Fire3") && canDash)
        {
            StopCoroutine("Dash");
            StartCoroutine("Dash");
        }

        // Check if we've dropped from a wall slide or ran off a platform without jumping, and reset
        // double jump and dash perms

        if (wasSlidingLastFrame && !isSliding && !isWallJumping)
        {
            canDoubleJump = true;
            canDash = true;
        }

        if(wasGroundedLastFrame && !isGrounded && rb.velocity.y <= 0)
        {
            canDoubleJump = true;
        }

        wasGroundedLastFrame = isGrounded;
        wasSlidingLastFrame = isSliding;

        // Applies lateral movement according to the moveNormal
        // movement variable used to be a literal force but now determines
        // whether to move or not based on some other factors
        if (movement != 0 && !isSliding)
        {
            float speedFactor = 2;

            if (!isGrounded)
            {
                speedFactor = 2;
            }

            // Movement formula
            transform.Translate(new Vector3(moveNormal.x * 2, moveNormal.y * 2, 0) * normDirFactor * dashSpeed * Time.deltaTime);

        }

        Debug.DrawRay(transform.position, moveNormal * normDirFactor, Color.green);
        //Debug.Log(canDash);
        

    }

    void FixedUpdate()
    {
        SetAnimParams();
    }
}
