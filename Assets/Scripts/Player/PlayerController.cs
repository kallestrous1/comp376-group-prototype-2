using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    Animator ani;
    public float speed;
    public float jumpHeight;
    public float lowJumpMultiplier = 2f;
    float xInput;

    bool isGrounded = false;
    public Transform isGroundedChecker;
    public float checkGroundRadius;
    public LayerMask groundLayer;

    public int JUMPCOUNT;
    public int jumpCount;
    bool jumpRequest = false;
    bool boost;

    public float coyoteTime = 0.2f;
    public float coyoteTimer;

    float rotationX;
    public float dashTime;
    bool isDashing;
    bool dashRequest;
    public float dashPower;
    public float DASHCOUNT;
    float dashCount;

    bool flipped = false;

    PlayerSound playerSound;

    public GameObject doubleJumpEffect;

    private KnockBack knockback;

    private bool checkBoost;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        knockback = GetComponent<KnockBack>();
        playerSound = GetComponent<PlayerSound>();
    }

    private void OnEnable()
    {
        InputManager.Instance.OnJumpPressed += HandleJumpPressed;
        InputManager.Instance.OnJumpReleased += HandleJumpReleased;
        InputManager.Instance.OnDashPressed += HandleDashPressed;
    }

    private void OnDisable()
    {
        InputManager.Instance.OnJumpPressed -= HandleJumpPressed;
        InputManager.Instance.OnJumpReleased -= HandleJumpReleased;
        InputManager.Instance.OnDashPressed -= HandleDashPressed;
    }

    #region InputHandlers
    private void HandleJumpPressed()
    {
        jumpRequest = true;
        checkBoost = true;
    }

    private void HandleDashPressed()
    {
        if (!isDashing)
        {
            dashRequest = true;
        }
    }

    private void HandleJumpReleased()
    {
        checkBoost = false;
        boost = false;
    }
    #endregion

    private void Update()
    {
        if (knockback.isBeingKnockedBack) return;

        CheckIfGrounded();
        xInput = InputManager.Instance.Move;
        ani.SetBool("IsDashing", isDashing);

        if (checkBoost)
        {
            boost = rb.linearVelocityY > 0 && !isDashing && !isGrounded;
        }
    }

    void FixedUpdate()
    {
        if (knockback.isBeingKnockedBack) return;

        XMovement();

        if (isGrounded)
        {
            ani.SetFloat("MoveX", Mathf.Abs(xInput));
            ani.SetFloat("MoveY", 0);
            dashCount = DASHCOUNT;
            jumpCount = JUMPCOUNT;
            coyoteTimer = coyoteTime;
        }
        else
        {
            ani.SetFloat("MoveY", rb.linearVelocity.y > 0 ? 1 : 0);
            ani.SetFloat("MoveX", 0);
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpRequest)
        {
            jumpRequest = false;
            Jump();
            coyoteTimer = 0;
        }
        if (boost)
        {
            rb.AddForce(new Vector2(0, lowJumpMultiplier), ForceMode2D.Impulse);
        }
        if (dashRequest)
        {
            dashRequest = false;
            StartCoroutine(Dash(dashPower, dashTime));
        }
    }

    #region XMovementandRotation
    void XMovement()
    {
        if (xInput > 0)
        {
            if (!flipped)
            {
                flipped = true;
                Vector2 currentPosition = transform.position;
                transform.position = new Vector2(currentPosition.x + 1, currentPosition.y);
                transform.Rotate(0f, 180f, 0f);
            }
            rotationX = 1;
        }
        else if (xInput < 0)
        {
            if (flipped)
            {
                flipped = false;
                Vector2 currentPosition = transform.position;
                transform.position = new Vector2(currentPosition.x - 1, currentPosition.y);
                transform.Rotate(0f, -180f, 0f);
            }
            rotationX = -1;
        }
        if (!isDashing)
        {
            float airMultiplier = isGrounded ? 1f : 0.5f;
            rb.AddForce(new Vector2(xInput * speed * airMultiplier, 0), ForceMode2D.Impulse);
        }
    }
    #endregion

    #region Dash
    IEnumerator Dash(float speed, float time)
    {
        if (dashCount > 0)
        {
            dashCount--;
            playerSound.PlayDashSound();
            isDashing = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(new Vector2(rotationX * speed, 0), ForceMode2D.Impulse);
            float gravity = rb.gravityScale;
            rb.gravityScale = 0;
            yield return new WaitForSeconds(time);
            isDashing = false;
            rb.gravityScale = gravity;
        }
    }
    #endregion

    #region Jump
    public void Jump()
    {
        if ((isGrounded || coyoteTimer > 0) && !isDashing)
        {
            rb.AddForce(new Vector2(0, jumpHeight), ForceMode2D.Impulse);
        }
        else if (jumpCount > 0 && !isDashing)
        {
            jumpCount--;
            if (doubleJumpEffect)
            {
                Instantiate(doubleJumpEffect, isGroundedChecker.position, Quaternion.identity);
            }
            ani.SetTrigger("DoubleJump");
            playerSound.PlayExtraJumpSound();
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(new Vector2(0, jumpHeight), ForceMode2D.Impulse);
        }
    }
    #endregion

    void CheckIfGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(isGroundedChecker.position, checkGroundRadius, groundLayer) != null;
    }
}
