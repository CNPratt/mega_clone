using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PM2 : MonoBehaviour
{
    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public Animator anim;
    public GameObject shotPoint;
    public GameObject bullet;

    private bool facingRight = true;
    public bool isGrounded = false;
    public bool isShooting = false;
    public bool isSliding = false;
    public bool canSlide = true;
    public bool jump = false;
    public bool fireButton = false;

    public float movement = 0f;
    public float horizontal = 0f;

    int slideMask;

    public Vector3 playerVelocity = new Vector2(0, 0);
    public Vector3 vec = new Vector3(0, 0, 0);

    public Vector2 slideJumpVec = new Vector2(0, 1);
    public Vector2 jumpInitialVec = new Vector3(0, .5f, 0);
    public Vector2 jumpVec = new Vector2(0, 250);

    List<RaycastHit2D> lateralHits;
    List<RaycastHit2D> groundHits;
    RaycastHit2D rightTopHit;
    RaycastHit2D rightMiddleHit;
    RaycastHit2D rightBottomHit;
    RaycastHit2D leftTopHit;
    RaycastHit2D leftMiddleHit;
    RaycastHit2D leftBottomHit;

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

    void InterpretHorizontalInput(bool isRight)
    {
        bool isFacingRight = true;
        bool shouldFlipSprite = false;
        float thisMovement = 1f;
        float shotpointOffset = -.6f;
        RaycastHit2D lateralTopHit = rightTopHit;
        RaycastHit2D lateralMidHit = rightMiddleHit;
        RaycastHit2D lateralBottomHit = rightBottomHit;
        Vector2 wallslideJumpVec = new Vector2(-4, 6);
        Vector3 jumpInitVec = new Vector3(-.1f, .1f, 0);

        if (rightBottomHit || rightMiddleHit || rightTopHit)
        {
            thisMovement = 0f;
        }

        if (!isRight)
        {
            // Is holding directional left input down

            isFacingRight = false;
            shouldFlipSprite = true;
            thisMovement = -1f;
            shotpointOffset = .6f;
            lateralTopHit = leftTopHit;
            lateralMidHit = leftMiddleHit;
            lateralBottomHit = leftBottomHit;
            wallslideJumpVec = new Vector2(4, 6);

            if (leftBottomHit || leftMiddleHit || leftTopHit)
            {
                thisMovement = 0f;
            }
        }

        movement = thisMovement;
        facingRight = isFacingRight;
        sprite.flipX = shouldFlipSprite;

        if (isSliding)
        {
            shotPoint.transform.position = new Vector3(rb.position.x + shotpointOffset, rb.position.y, 0);
        }
        else
        {
            shotPoint.transform.position = new Vector3(rb.position.x + -shotpointOffset, rb.position.y, 0);
        }

        if (lateralMidHit.collider == true && !isGrounded)
        {
            //if (canSlide)
            //{
                isSliding = true;
            //}

            slideJumpVec = wallslideJumpVec;
            jumpInitialVec = jumpInitVec;
        }
        else
        {
            isSliding = false;
        }
    }

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

    private List<RaycastHit2D> GroundedCasts()
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        Debug.DrawRay(transform.position + new Vector3(-.24f, -.5f, 0), -Vector2.up / 100);
        RaycastHit2D bottomLeft = Physics2D.Raycast(transform.position + new Vector3(-.24f, -.5f, 0), -Vector2.up, .01f, slideMask);
        hits.Add(bottomLeft);

        Debug.DrawRay(transform.position + new Vector3(0, -.5f, 0), -Vector2.up / 100);
        RaycastHit2D bottomMid = Physics2D.Raycast(transform.position + new Vector3(0, -.5f, 0), -Vector2.up, .01f, slideMask);
        hits.Add(bottomMid);

        Debug.DrawRay(transform.position + new Vector3(.24f, -.5f, 0), -Vector2.up / 100);
        RaycastHit2D bottomRight = Physics2D.Raycast(transform.position + new Vector3(.24f, -.5f, 0), -Vector2.up, .01f, slideMask);
        hits.Add(bottomRight);

        return hits;
    }

    private List<RaycastHit2D> LateralCasts()
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();

        RaycastHit2D rightTop = Physics2D.Raycast(transform.position + new Vector3(.25f, .5f, 0), -Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + new Vector3(.25f, .5f, 0), new Vector2(.01f, 0));
        hits.Add(rightTop);

        RaycastHit2D rightMid = Physics2D.Raycast(transform.position + new Vector3(.25f, 0, 0), -Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + new Vector3(.25f, 0, 0), new Vector2(.01f, 0));
        hits.Add(rightMid);

        RaycastHit2D rightBottom = Physics2D.Raycast(transform.position + new Vector3(.25f, -.5f, 0), -Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + new Vector3(.25f, -.5f, 0), new Vector2(.01f, 0));
        hits.Add(rightBottom);

        RaycastHit2D leftTop = Physics2D.Raycast(transform.position + new Vector3(-.25f, .5f, 0), Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + new Vector3(-.25f, .5f, 0), new Vector2(-.01f, 0));
        hits.Add(leftTop);

        RaycastHit2D leftMid = Physics2D.Raycast(transform.position + new Vector3(-.25f, 0, 0), Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + new Vector3(-.25f, 0, 0), new Vector2(-.01f, 0));
        hits.Add(leftMid);

        RaycastHit2D leftBottom = Physics2D.Raycast(transform.position + new Vector3(-.25f, -.5f, 0), Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + new Vector3(-.25f, -.5f, 0), new Vector2(-.01f, 0));
        hits.Add(leftBottom);

        return hits;
    }

    private IEnumerator SetIsShootingToFalse()
    {
        yield return new WaitForSeconds(1f);
        if (isShooting)
        {
            isShooting = false;
        }
    }

    private IEnumerator CanSlideToTrue()
    {
        yield return new WaitForSeconds(.1f);
        if (!canSlide)
        {
            canSlide = true;
        }
    }

    private IEnumerator WallJumpDeflector()
    {
        yield return new WaitForSeconds(.3f);
        rb.AddForce(new Vector2(-slideJumpVec.x, 2), ForceMode2D.Impulse);
    }

    private IEnumerator JumpRoutine()
    {
        rb.velocity = Vector2.zero;
        Vector2 jumpVector = rb.position;
        float jumpTime = .2f;
        float timer = 0;

        while (Input.GetButton("Jump") && timer < jumpTime)
        {
            float proportionCompleted = timer / jumpTime;
            Vector2 thisFrameJumpVector = Vector2.Lerp(Vector2.up, Vector2.zero, proportionCompleted);
            rb.AddForce(thisFrameJumpVector * Time.deltaTime * 50, ForceMode2D.Impulse);
            //transform.Translate(thisFrameJumpVector/10);
            timer += Time.deltaTime;
            yield return null;
        }

        // jumping = false;
    }

    private IEnumerator Walljump()
    {
        rb.velocity = Vector2.zero;
        Vector2 jumpVector = rb.position;
        float jumpTime = .2f;
        float timer = 0;

        while (timer < jumpTime)
        {
            float proportionCompleted = timer / jumpTime;
            Vector2 thisFrameJumpVector = Vector2.Lerp(Vector2.up, slideJumpVec, proportionCompleted);
            //rb.AddForce(thisFrameJumpVector * Time.deltaTime * 50, ForceMode2D.Impulse);
            transform.Translate(thisFrameJumpVector / 50);
            timer += Time.deltaTime;
            yield return null;
        }

        // jumping = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        sprite = gameObject.GetComponent<SpriteRenderer>();
        anim = gameObject.GetComponent<Animator>();

        slideMask = LayerMask.GetMask("Main Tilemap Layer");
    }

    // Update is called once per frame
    void Update()
    {
        movement = 0f;

        horizontal = Input.GetAxisRaw("Horizontal");
        jump = Input.GetButtonDown("Jump");
        fireButton = Input.GetButtonDown("Fire1");
        playerVelocity = rb.velocity;

        SetAnimParams();

        // Grounding raycast
        groundHits = GroundedCasts();

        // Sliding raycasts
        lateralHits = LateralCasts();
        rightMiddleHit = lateralHits[1];

        leftMiddleHit = lateralHits[4];

        //Debug.Log(groundHits[1]);

        //if (groundHits[1].collider == true)
        //{
        //    isGrounded = true;
        //}
        //else
        //{
        //    isGrounded = false;
        //}

        foreach (RaycastHit2D hit in groundHits)
        {
            if (hit.collider == true)
            {
                isGrounded = true;
                break;
            }
            else
            {
                isGrounded = false;
            }
        }

        if (isSliding)
        {
            rb.drag = 20f;
        }
        else
        {
            rb.drag = 0f;
        }

        if (fireButton)
        {
            Shoot();
        }

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

        vec = new Vector3(rb.position.x + movement, rb.position.y, 0);

        if (jump && isGrounded)
        {
            //canSlide = false;
            //StopCoroutine("CanSlideToTrue");
            //StartCoroutine("CanSlideToTrue");
            StartCoroutine("JumpRoutine");
            // rb.AddForce(jumpVec);
        }

        if (jump && isSliding)
        {
            rb.AddForce(slideJumpVec, ForceMode2D.Impulse);
            //StopCoroutine("Walljump");
            //StartCoroutine("Walljump");
            // StopCoroutine("WallJumpDeflector");
            // StartCoroutine("WallJumpDeflector");
        }

        if (movement != 0 && !isSliding)
        {
            float speedFactor = 2;

            if (!isGrounded)
            {
                speedFactor = 2;
            }
            transform.Translate(new Vector3(movement * speedFactor, 0, 0) * Time.deltaTime);

        }

        //if (movement == 0)
        //{
        //    rb.velocity = new Vector2(0, rb.velocity.y);
        //}
    }

    void FixedUpdate()
    {

    }
}
