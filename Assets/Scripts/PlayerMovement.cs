using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Visuals")]
    Animator myAnimator;
    SpriteRenderer myRenderer;

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
    bool aliveState = true;
    bool damagedState = false;
    bool climbingState = false;
    bool disabledState = false;


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
        if (!aliveState) { return; }
        if (disabledState) { return; }
        Run();
        ClimbLadder();
        FlipSprite();
    }

    void OnMove(InputValue value)
    {
        if (!aliveState) { return; }
        moveInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (!aliveState) { return; }
        if (!IsTouchingPlatform()) { return; }

        if (value.isPressed)
        {
            myRigidbody.velocity += new Vector2 (0f, jumpSpeed);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!aliveState) { return; }
        if (!damagedState && other.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(other.gameObject);
        }
    }

    void TakeDamage(GameObject enemy)
    {
        hp--;

        // small bounce from damage impact
        Rigidbody2D enemyRigidbody = enemy.GetComponent<Rigidbody2D>();
        Vector2 impulse = new Vector2 (Mathf.Sign(enemyRigidbody.velocity.x) * 5, 5);
        myRigidbody.velocity = new Vector2 (0, 0);
        myRigidbody.AddForce(impulse, ForceMode2D.Impulse);

        if (hp > 0)
        {
            StartCoroutine(SetTimerDamagedState(3.0f));
            StartCoroutine(SetTimerDisabledState(0.5f));
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
        aliveState = false;
        myAnimator.SetTrigger("Dying");
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
        if (IsTouchingLadder() && !climbingState && (Mathf.Abs(moveInput.y) > Mathf.Epsilon))
        {
            climbingState = true;
            myRigidbody.gravityScale = 0f;
        }
        // exit criteria: was climbing beforehand, stops touching ladder
        else if (!IsTouchingLadder() && climbingState)
        {
            climbingState = false;
            myRigidbody.gravityScale = defaultGravityScale;
        }

        myAnimator.SetBool("isClimbing", climbingState);
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Platform"), climbingState);

        if (!climbingState) { return; }

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

    bool IsTouchingPlatform()
    {
        return myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Platform"));
    }

    IEnumerator SetTimerDamagedState(float seconds)
    {
        damagedState = true;
        yield return new WaitForSeconds(seconds);
        damagedState = false;
    }

    IEnumerator SetTimerDisabledState(float seconds)
    {
        disabledState = true;
        yield return new WaitForSeconds(seconds);
        disabledState = false;
    }
}