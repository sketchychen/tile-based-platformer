using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1f;
    Rigidbody2D myRigidbody;
    BoxCollider2D myBoxCollider2D;
    CapsuleCollider2D myBodyCollider2D;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myBoxCollider2D = GetComponent<BoxCollider2D>();
        myBodyCollider2D = GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveEnemy();
        FlipSprite();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("OnTriggerExit");
        moveSpeed = -moveSpeed;
    }

    void MoveEnemy()
    {
        myRigidbody.velocity = new Vector2 (moveSpeed, 0f);
    }

    void FlipSprite()
    {
        transform.localScale = new Vector2 (Mathf.Sign(myRigidbody.velocity.x), 1f);
    }
}
