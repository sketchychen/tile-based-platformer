using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Visuals")]
    Animator myAnimator;
    SpriteRenderer myRenderer;

    [Header("GameObjects")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform gun;

    [Header("Kinetics")]
    Rigidbody2D myRigidbody;
    Vector2 moveInput;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;
    float moveSpeed = 5f;
    float jumpSpeed = 10f;
    float climbSpeed = 5f;
    float defaultGravityScale = 2f;

    [Header("Player Status")]
    int hp = 3;

    [Header("Player Condition Boolean")]
    /* State Entry => State Changes => State Exit => State Reversion */
    bool isAlive = true;
    bool isDamaged = false;
    bool isClimbing = false;
    bool isDisabled = false;
    bool isAiming = false;
    bool isShooting = false;



    void Awake()
    {
        myAnimator = GetComponent<Animator>();
        myRenderer = GetComponent<SpriteRenderer>();
        myRigidbody = GetComponent<Rigidbody2D>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (!isAlive) { return; }
        if (isDisabled) { return; }
        Run();
        ClimbLadder();
        FlipSprite();
    }

    void OnFire(InputValue value) {
        if (!isAlive) { return; }
        myAnimator.SetBool("isShooting", true);
        // Invoke("TakeShot", 0.3f); // shooting's animation arrow release is at 0.3s
        TakeShot();
        /*
        there's no aiming here but
        consider animation for aiming and animation for shooting next time
        */
    }

    void OnMove(InputValue value)
    {
        if (!isAlive) { return; }
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (!isAlive) { return; }
        if (!IsTouchingGround()) { return; }

        if (value.isPressed)
        {
            myRigidbody.velocity += new Vector2 (0f, jumpSpeed);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!isAlive) { return; }
        if (!isDamaged && IsTouchingDamage())
        {
            DamageRecoil(other.gameObject);
            // TakeDamage();
            Die();
        }
    }

    void TakeShot()
    {
        Instantiate(bullet, gun.position, transform.rotation);
        Invoke("StowWeapon", 0.417f);  // shooting's animation is 0.417s total
    }

    void StowWeapon()
    {
        myAnimator.SetBool("isShooting", false);
    }

    void DamageRecoil(GameObject gameObject)
    {
        Vector2 impulse;
        if (!HasHorizontalVelocity() && gameObject.CompareTag("Enemy"))
        {
            Rigidbody2D enemyRigidBody = gameObject.GetComponent<Rigidbody2D>();
            impulse = new Vector2 (Mathf.Sign(enemyRigidBody.velocity.x) * 5, 5);
        }
        else
        {
            impulse = new Vector2 (Mathf.Sign(myRigidbody.velocity.x) * -5, 5);
        }
        myRigidbody.velocity = new Vector2 (0, 0);
        myRigidbody.AddForce(impulse, ForceMode2D.Impulse);
    }

    void TakeDamage()
    {
        hp--;
        if (hp > 0)
        {
            StartCoroutine(SetTimerIsDamaged(3.0f));
            StartCoroutine(SetTimerIsDisabled(0.5f));
            StartCoroutine(BlinkPlayer(9, 3.0f));
        }
        else
        {
            Die();
        }
    }

    IEnumerator BlinkPlayer(int cycles, float seconds)
    {
        float interval = seconds / cycles / 2;
        for(var n = 0; n < cycles; n++)
        {
            myRenderer.material.color = Color.white;
            yield return new WaitForSeconds(interval);
            myRenderer.material.color = Color.gray;
            yield return new WaitForSeconds(interval);
        }
        myRenderer.material.color = Color.white;
    }

    void Die()
    {
        isAlive = false;
        isDisabled = true;
        myAnimator.SetTrigger("Dying");
        FindObjectOfType<GameSession>().ProcessPlayerDeath();
    }

    void Run()
    {
        Vector2 playerVelocity = new Vector2 (moveInput.x * moveSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;

        myAnimator.SetBool("isRunning", HasHorizontalVelocity());
    }

    void ClimbLadder()
    {
        // entry criteria: touching ladder, wasn't climbing beforehand, presses "up"
        if (IsTouchingLadder() && !isClimbing && (Mathf.Abs(moveInput.y) > Mathf.Epsilon))
        {
            isClimbing = true;
            myRigidbody.gravityScale = 0f;
        }
        // exit criteria: was climbing beforehand, stops touching ladder
        else if (!IsTouchingLadder() && isClimbing)
        {
            isClimbing = false;
            myRigidbody.gravityScale = defaultGravityScale;
        }

        myAnimator.SetBool("isClimbing", isClimbing);
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Platform"), isClimbing);

        if (!isClimbing) { return; }

        Vector2 playerVelocity = new Vector2 (moveInput.normalized.x, moveInput.normalized.y) * climbSpeed;
        myRigidbody.velocity = playerVelocity;
        myAnimator.enabled = HasAnyVelocity();

    }

    void FlipSprite()
    {
        if (HasHorizontalVelocity())
        {
            transform.localScale = new Vector2 (Mathf.Sign(myRigidbody.velocity.x), 1f);
        }
    }

    bool HasHorizontalVelocity()
    {
        return Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
    }

    bool HasVerticalVelocity()
    {
        return Mathf.Abs(myRigidbody.velocity.y) > Mathf.Epsilon;
    }

    bool HasAnyVelocity()
    {
        return Mathf.Abs(myRigidbody.velocity.magnitude) > Mathf.Epsilon;
    }

    bool IsTouchingLadder()
    {
        LayerMask climbMask = LayerMask.GetMask("Climb");
        return myBodyCollider.IsTouchingLayers(climbMask) || myFeetCollider.IsTouchingLayers(climbMask);
    }

    bool IsTouchingGround()
    {
        LayerMask groundLayers = LayerMask.GetMask("Platform", "Hazard");
        return myFeetCollider.IsTouchingLayers(groundLayers);
    }

    bool IsTouchingDamage()
    {
        LayerMask damageLayers = LayerMask.GetMask("Enemy", "Hazard");
        return myBodyCollider.IsTouchingLayers(damageLayers) || myFeetCollider.IsTouchingLayers(damageLayers);
    }

    IEnumerator SetTimerIsDamaged(float seconds)
    {
        isDamaged = true;
        yield return new WaitForSeconds(seconds);
        isDamaged = false;
    }

    IEnumerator SetTimerIsDisabled(float seconds)
    {
        isDisabled = true;
        yield return new WaitForSeconds(seconds);
        isDisabled = false;
    }
}
