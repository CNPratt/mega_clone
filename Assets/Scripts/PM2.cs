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
    private bool isGrounded = false;
    public bool isShooting = false;
    public bool isSliding = false;
    public bool jump = false;
    public bool fireButton = false;

    public float movement = 0f;
    public float horizontal = 0f;

    int slideMask;

    public Vector3 groundRayOffset = new Vector3(0, 0.01f, 0);
    public Vector3 slideRayOffset = new Vector3(0, 0, 0);
    public Vector3 playerVelocity = new Vector2(0, 0);
    public Vector3 vec = new Vector3(0, 0, 0);

    public Vector2 slideJumpVec = new Vector2(0, 1);
    public Vector2 jumpInitialVec = new Vector3(0, .5f, 0);
    public Vector2 jumpVec = new Vector2(0, 250);

    RaycastHit2D groundHit;
    RaycastHit2D rightHit;
    RaycastHit2D leftHit;

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
        RaycastHit2D wallslideHit = rightHit;
        Vector2 wallslideJumpVec = new Vector2(-4, 6);
        Vector3 jumpInitVec = new Vector3(-.1f, .1f, 0);

        if (!isRight)
        {
            // Is holding directional right input down

            isFacingRight = false;
            shouldFlipSprite = true;
            thisMovement = -1f;
            shotpointOffset = .6f;
            wallslideHit = leftHit;
            wallslideJumpVec = new Vector2(4, 6);
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

        if (wallslideHit.collider == true && !isGrounded)
        {
            isSliding = true;
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

    private RaycastHit2D GroundedCast()
    {
        Debug.DrawRay(transform.position - (Vector3.up / 2) - groundRayOffset, -Vector2.up / 100);
        return Physics2D.Raycast(transform.position - (Vector3.up / 2), -Vector2.up, .01f, slideMask);
    }

    private List<RaycastHit2D> WallslideCasts()
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        RaycastHit2D rightMid = Physics2D.Raycast(transform.position - (Vector3.left / 4), -Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position - (Vector3.left / 4) + slideRayOffset, -Vector2.left / 100);
        RaycastHit2D leftMid = leftHit = Physics2D.Raycast(transform.position + (Vector3.left / 4), Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + (Vector3.left / 4) - slideRayOffset, Vector2.left / 100);

        hits.Add(rightMid);
        hits.Add(leftMid);

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

    private IEnumerator WallJumpDeflector()
    {
        yield return new WaitForSeconds(.3f);
        rb.AddForce(new Vector2(-slideJumpVec.x, 2), ForceMode2D.Impulse);
        Debug.Log("ran");
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
        groundHit = GroundedCast();

        // Sliding raycasts
        List<RaycastHit2D> wallslideHits = WallslideCasts();
        rightHit = wallslideHits[0];

        leftHit = wallslideHits[1];

        if (groundHit.collider == true)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
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
            StartCoroutine("JumpRoutine");
            // rb.AddForce(jumpVec);
        }

        if (jump && isSliding)
        {
            rb.AddForce(slideJumpVec, ForceMode2D.Impulse);
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

        if (movement == 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void FixedUpdated()
    {

    }
}
