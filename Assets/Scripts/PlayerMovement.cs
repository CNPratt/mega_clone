using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
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
        float movement = 0f;
        float horizontal = Input.GetAxisRaw("Horizontal");
        bool jump = Input.GetButtonDown("Jump");
        bool fireButton = Input.GetButtonDown("Fire1");
        Vector3 groundRayOffset = new Vector3(0, 0.01f, 0);
        Vector3 slideRayOffset = new Vector3(0, 0, 0);
        Vector3 playerVelocity = hero.velocity;
        Vector2 slideJumpVec = new Vector2(0, 250);
        Vector2 jumpInitialVec = new Vector3(0, .5f, 0);

        int slideMask = LayerMask.GetMask("Main Tilemap Layer");

        anim.SetFloat("Speed", Mathf.Abs(horizontal));
        anim.SetFloat("VerticalVelocity", playerVelocity.y);
        anim.SetBool("isShooting", isShooting);
        anim.SetBool("isSliding", isSliding);

        // Grounding raycast
        RaycastHit2D groundHit = Physics2D.Raycast(transform.position - (Vector3.up/2) - groundRayOffset, -Vector2.up, .05f);
        Debug.DrawRay(transform.position - (Vector3.up/2) - groundRayOffset, -Vector2.up/20);

        // Sliding raycasts
        RaycastHit2D rightHit = Physics2D.Raycast(transform.position - (Vector3.left/4) + slideRayOffset, -Vector2.left, .01f, slideMask);
        Debug.DrawRay(transform.position - (Vector3.left/4) + slideRayOffset, - Vector2.left/100);

        RaycastHit2D leftHit = Physics2D.Raycast(transform.position + (Vector3.left/4) - slideRayOffset, Vector2.left, .01f, slideMask);
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
            Vector2 forward = facingRight ? -Vector2.left : Vector2.left;
            isShooting = true;
            StopCoroutine("SetIsShootingToFalse");
            StartCoroutine("SetIsShootingToFalse");
            bullet = Instantiate(bullet, shotPoint.transform.position, Quaternion.identity);
            bullet.GetComponent<BaseShotScript>().direction = forward * 10;
        }

        if(horizontal > .1f) {
            movement = .04f;
            facingRight = true;
            sprite.flipX = false;
            shotPoint.transform.position = new Vector3(hero.position.x + .6f, hero.position.y, 0);

            if(rightHit.collider == true && !isGrounded) {
                isSliding = true;
                slideJumpVec = new Vector2(-250, 250);
                jumpInitialVec = new Vector3(-.3f, .3f, 0);
            } else {
                isSliding = false;
            }
        } else if(horizontal < -.1f) {
            movement = -.04f;
            facingRight = false;
            sprite.flipX = true;
            shotPoint.transform.position = new Vector3(hero.position.x - .6f, hero.position.y, 0);

            if(leftHit.collider == true && !isGrounded) {
                isSliding = true;
                slideJumpVec = new Vector2(250, 400);
                jumpInitialVec = new Vector3(.3f, .3f, 0);
            } else {
                isSliding = false;
            }
        } else {
            isSliding = false;
        }
        Vector2 vec = new Vector3(hero.position.x + movement, hero.position.y, 0);
        Vector2 jumpVec = new Vector2(0, 250);

        if(jump && isGrounded) {
            hero.AddForce(jumpVec);
        }

        if(jump && isSliding) {
            hero.position = hero.position + jumpInitialVec;
            hero.AddForce(slideJumpVec);
        }

        if(movement != 0 && !isSliding){
            hero.position = vec;
        }

        Debug.Log(isSliding);
    }
}
