using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float fallThreshold = -10f;
    [SerializeField] private Slider forceBar; // Kéo Slider vào đây
    [SerializeField] private float chargeRate = 0.3f; // Tốc độ tăng

    public Vector3 respawnPosition;
    private bool isJumping = false;
    private float chargeValue = 0f;
    private bool isCharging = false;
    private KeyCode currentChargeKey = KeyCode.None;

    public Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private Vector3 originalScale;
    private bool canControl = false;
    private float wallJumpCooldown;
    private float horizontalInput;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
        respawnPosition = transform.position;
        originalScale = transform.localScale;

        if (forceBar != null)
        {
            forceBar.minValue = 0f;
            forceBar.maxValue = 1f;
            forceBar.value = 0f;
        }
    }

    private void Update()
    {
        HandleForceCharge();
        if (!canControl) return;

        horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }

        anim.SetBool("run", horizontalInput != 0);

        if (wallJumpCooldown > 0.2f)
        {
            if (!isJumping)
            {
                rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
            }

            if (onWall() && isJumping)
            {
                rb.gravityScale = 0;
                rb.velocity = Vector2.zero;
            }
            else
            {
                rb.gravityScale = 7;
            }

            if (Input.GetKey(KeyCode.UpArrow) && !isJumping)
            {
                Jump();
            }
        }
        else
        {
            wallJumpCooldown += Time.deltaTime;
        }

        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    public void Move(float direction)
    {
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

        if (direction > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (direction < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }

        anim.SetBool("run", direction != 0);
    }

    public void Jump()
    {
        if (!isJumping)
        {
            isJumping = true;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            anim.SetTrigger("jump");
        }
        else if (onWall())
        {
            isJumping = true;
            if (horizontalInput == 0)
            {
                rb.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 10, 0);
                transform.localScale = new Vector3(-Mathf.Sign(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                rb.velocity = new Vector2(-Mathf.Sign(transform.localScale.x) * 3, 6);
            }
            wallJumpCooldown = 0;
        }
    }

    public void JumpAndMove(float horizontalDirection)
    {
        if (isJumping == false) // Chỉ cho nhảy khi đang chạm đất
        {
            rb.velocity = new Vector2(horizontalDirection * moveSpeed, jumpForce);

            // Lật hướng nhân vật
            if (horizontalDirection > 0.01f)
            {
                transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z); // Quay phải
            }
            else if (horizontalDirection < -0.01f)
            {
                transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z); // Quay trái
            }

            anim.SetBool("run", horizontalDirection != 0);
            anim.SetTrigger("jump");
        }
    }

    private void HandleForceCharge()
    {
        Debug.Log("Handling force charge...");  // Check if the method is being called

        // Kiểm tra nếu giữ phím LeftArrow
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // Nếu trước đó không giữ LeftArrow thì reset
            if (currentChargeKey != KeyCode.LeftArrow)
            {
                chargeValue = 0f;  // Reset thanh
                currentChargeKey = KeyCode.LeftArrow;
            }
            ChargeForceBar();  // Tăng dần
        }
        // Kiểm tra nếu giữ phím RightArrow
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            // Nếu trước đó không giữ RightArrow thì reset
            if (currentChargeKey != KeyCode.RightArrow)
            {
                chargeValue = 0f;  // Reset thanh
                currentChargeKey = KeyCode.RightArrow;
            }
            ChargeForceBar();  // Tăng dần
        }
        else
        {
            // Không giữ phím nào → reset thanh
            chargeValue = 0f;
            currentChargeKey = KeyCode.None;

            if (forceBar != null)
            {
                forceBar.value = chargeValue;
            }
        }
    }


    // Hàm tăng thanh theo thời gian
    private void ChargeForceBar()
    {
        chargeValue += chargeRate * Time.deltaTime;  // Tăng dần theo thời gian giữ
        chargeValue = Mathf.Clamp01(chargeValue);    // Giới hạn từ 0 đến 1

        if (forceBar != null)
        {
            forceBar.value = chargeValue;  // Cập nhật thanh Slider
            Debug.Log("Force Bar Value: " + forceBar.value);  // Log updated slider value
        }
    }




    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
        }

        if (collision.gameObject.CompareTag("DeathObs"))
        {
            Die();
        }

        if (collision.gameObject.CompareTag("Bullet"))
        {
            Die();
        }
    }

 

    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }

    public void Die()
    {
        Debug.Log("Player is Dead!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void Respawn()
    {
        transform.position = respawnPosition;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        anim.SetBool("run", false);
        anim.SetBool("grounded", true);
    }

    public void SetControl(bool value)
    {
        canControl = value;
        if (!value)
        {
            anim.SetBool("run", false);
        }
    }

    public void ResetHorizontalVelocity()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);  // Chỉ reset trục X
    }

    public void SetRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }
}
