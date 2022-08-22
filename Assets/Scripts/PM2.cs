using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PM2 : MonoBehaviour
{
    public Rigidbody2D hero;
    public SpriteRenderer sprite;
    public Animator anim;
    public GameObject shotPoint;
    public GameObject bullet;
    
    private bool facingRight = true;
    private bool isGrounded = false;
    public bool isShooting = false;
    public bool isSliding = false;

    public float movement = 0f;
    public float horizontal = 0f;
    public bool jump = false;
    public bool fireButton = false;
    public Vector3 groundRayOffset = new Vector3(0, 0.01f, 0);
    public Vector3 slideRayOffset = new Vector3(0, 0, 0);
    public Vector3 playerVelocity = new Vector2(0, 0);
    public Vector2 slideJumpVec = new Vector2(0, 1);
    public Vector2 jumpInitialVec = new Vector3(0, .5f, 0);
    public Vector3 vec = new Vector3(0, 0, 0);
    public Vector2 jumpVec = new Vector2(0, 250);

    private IEnumerator SetIsShootingToFalse () {
        yield return new WaitForSeconds(1f);
        if(isShooting) {
            isShooting = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        hero = gameObject.GetComponent<Rigidbody2D>();
        sprite = gameObject.GetComponent<SpriteRenderer>();
        anim = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        movement = 0f;
        horizontal = Input.GetAxisRaw("Horizontal");
        jump = Input.GetButtonDown("Jump");
        fireButton = Input.GetButtonDown("Fire1");
        groundRayOffset = new Vector3(0, 0.01f, 0);
        slideRayOffset = new Vector3(0, 0, 0);
        playerVelocity = hero.velocity;
        slideJumpVec = new Vector2(0, 1);
        jumpInitialVec = new Vector3(0, .5f, 0);
        jumpVec = new Vector2(0, 250);

        int slideMask = LayerMask.GetMask("Main Tilemap Layer");

        anim.SetFloat("Speed", Mathf.Abs(horizontal));
        anim.SetFloat("VerticalVelocity", playerVelocity.y);
        anim.SetBool("isShooting", isShooting);
        anim.SetBool("isSliding", isSliding);

        // Grounding raycast
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position - (Vector3.up/2), -Vector2.up, .01f, slideMask);
        Debug.DrawRay(transform.position - (Vector3.up/2) - groundRayOffset, -Vector2.up/100);

        // Sliding raycasts
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position - (Vector3.left/4), -Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position - (Vector3.left/4) + slideRayOffset, - Vector2.left/100);

        RaycastHit2D leftHit = Physics2D.Raycast(transform.position + (Vector3.left/4), Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position + (Vector3.left/4) - slideRayOffset, Vector2.left/100);

        if (groundHit.collider == true) {
            isGrounded = true;
            anim.SetBool("isGrounded", true);
        } else {
            isGrounded = false;
            anim.SetBool("isGrounded", false);
        }

        if(isSliding) {
            hero.drag = 20f;
        } else {
            hero.drag = 0f;
        }

        if(fireButton) {
            GameObject newBullet;
            Vector2 forward = facingRight ? -Vector2.left : Vector2.left;
            isShooting = true;
            StopCoroutine("SetIsShootingToFalse");
            StartCoroutine("SetIsShootingToFalse");
            newBullet = Instantiate(bullet, shotPoint.transform.position, Quaternion.identity);
            if(isSliding) {
                newBullet.GetComponent<BaseShotScript>().direction = -forward * 10;
            } else {
                newBullet.GetComponent<BaseShotScript>().direction = forward * 10;
            }
            
        }

        if(horizontal > .1f) {
            movement = 1f;
            facingRight = true;
            sprite.flipX = false;
            if(isSliding) {
                shotPoint.transform.position = new Vector3(hero.position.x + -.6f, hero.position.y, 0);
            } else {
                shotPoint.transform.position = new Vector3(hero.position.x + .6f, hero.position.y, 0);
            }

            if(rightHit.collider == true && !isGrounded) {
                isSliding = true;
                slideJumpVec = new Vector2(-2, 10);
                jumpInitialVec = new Vector3(-.1f, .1f, 0);
            } else {
                isSliding = false;
            }
        } else if(horizontal < -.1f) {
            movement = -1f;
            facingRight = false;
            sprite.flipX = true;
            if(isSliding) {
                shotPoint.transform.position = new Vector3(hero.position.x + .6f, hero.position.y, 0);
            } else {
                shotPoint.transform.position = new Vector3(hero.position.x + -.6f, hero.position.y, 0);
            }

            if(leftHit.collider == true && !isGrounded) {
                isSliding = true;
                slideJumpVec = new Vector2(2, 10);
                jumpInitialVec = new Vector3(.1f, .1f, 0);
            } else {
                isSliding = false;
            }
        } else {
            isSliding = false;
        }

        vec = new Vector3(hero.position.x + movement, hero.position.y, 0);

        if(jump && isGrounded) {
            hero.AddForce(jumpVec);
        }

        if(jump && isSliding) {
            // hero.position = hero.position + jumpInitialVec;
            // transform.Translate(jumpInitialVec);
            hero.AddForce(slideJumpVec, ForceMode2D.Impulse);
        }

        if(movement != 0 && !isSliding){
            float speedFactor = 2;

            if(!isGrounded) {
                speedFactor = 2;
            }
            // hero.position = vec;
            // hero.AddForce(new Vector2(movement * speedFactor, 0), ForceMode2D.Impulse);
            transform.Translate(new Vector3(movement * speedFactor, 0, 0 ) * Time.deltaTime);
        }

        if(movement == 0) {
            hero.velocity = new Vector2(0, hero.velocity.y);
        }

        Debug.Log(isSliding);
    }

    void FixedUpdated() {
        
    }
}
