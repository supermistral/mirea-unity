using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int RotateValue;

    [SerializeField]
    private float _jumpForce, _moveForce, _gravityForce;

    [SerializeField]
    private float _groundDrag, _airDrag, _searchRadius;

    [SerializeField]
    private LayerMask _groundMask, _gravityMask;

    [SerializeField]
    private AnimationClip _idle, _walk, _jump, _fall;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;
    private Vector3 gravityUpDirection, moveDirection;
    private GameObject previousGravityObject;

    private float horizontalInput;
    private bool isGrounded;

    private void Start()
    {
        Physics2D.queriesHitTriggers = true;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        gravityUpDirection = Vector3.up;
        moveDirection = Vector3.right;
        previousGravityObject = gameObject;

        RotateValue = 0;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        GetGravityDirection();

        // setting gravity
        moveDirection = Vector3.Cross(Vector3.back, gravityUpDirection);
        float velocityY = Vector3.Dot(rb.velocity, gravityUpDirection);
        float velocityX = Vector3.Dot(rb.velocity, moveDirection);

        float forceValue = _gravityForce;
        rb.AddForce(-Time.deltaTime * forceValue * gravityUpDirection);

        // checking ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -gravityUpDirection, 1f, _groundMask);
        isGrounded = hit.collider != null;
    
        // moving horizontally
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            forceValue = _moveForce;
            rb.AddForce(Time.deltaTime * horizontalInput * forceValue * moveDirection);
        }
        else
        {
            float drag = isGrounded ? _groundDrag : _airDrag;
            forceValue = velocityX;
            rb.AddForce(-drag * forceValue * moveDirection);
        }

        // rotation for platforms
        if (RotateValue != 0)
        {
            forceValue = _moveForce;
            rb.AddForce(Time.deltaTime * RotateValue * moveDirection * forceValue);
        }

        // jumping
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(_jumpForce * gravityUpDirection, ForceMode2D.Impulse);
        }

        sr.flipX = Mathf.Abs(velocityX) > 0.05 ? velocityX < -0.05f : sr.flipX;

        if (!isGrounded)
        {
            animator.Play(velocityY > 0f ? _jump.name : _fall.name);
        }
        else if (RotateValue == 0)
        {
            animator.Play(Mathf.Abs(velocityX) > 0.2f ? _walk.name : _idle.name);
        }

        // setting velocity limit
        if (velocityY < -_gravityForce * Time.deltaTime * 4f)
        {
            forceValue = velocityY;
            rb.AddForce(-forceValue * gravityUpDirection);
        }

        if (Mathf.Abs(velocityX) > _moveForce * Time.deltaTime * 4f)
        {
            forceValue = velocityX;
            rb.AddForce(-forceValue * moveDirection);
        }
    }

    private void FixedUpdate()
    {
        Quaternion rotation = Quaternion.FromToRotation(transform.up, gravityUpDirection) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);    
    }

    public void GetGravityDirection()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, _searchRadius, Vector2.up, 0f, _gravityMask);
    
        if (hits.Length == 0) return;

        float closestDistance = _searchRadius;
        float tempDistance;
        GameObject closestObject = hits[0].collider.gameObject;

        foreach (var item in hits)
        {
            Vector3 itemPos = item.collider.gameObject.transform.position;
            tempDistance = Vector3.Distance(transform.position, itemPos);

            if (tempDistance < closestDistance)
            {
                closestDistance = tempDistance;
                closestObject = item.collider.gameObject;
            }
        }

        gravityUpDirection = closestObject.GetComponent<GravityDirection>().GetDirection(transform.position);
    
        if (closestObject != previousGravityObject)
        {
            GameManager.Instance.MoveCamera(closestObject.transform.position);
            previousGravityObject = closestObject;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Constants.Tags.DIAMOND))
        {
            GameManager.Instance.UpdateDiamond();
            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag(Constants.Tags.DEATH))
        {
            GameManager.Instance.RestartGame();
            Destroy(gameObject);
        }
        else if (collision.CompareTag(Constants.Tags.WIN))
        {
            GameManager.Instance.EndGame();
            Destroy(gameObject);
        }
    }
}
