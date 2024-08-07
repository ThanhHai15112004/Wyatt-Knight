using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings:")]
    [SerializeField] private float moveSpeed = 5f; // Toc do di chuyen
    private float xAxis; // Bien luu gia tri di chuyen theo truc x
    private float yAxis;
    private float gravity;
    [Space(5)]

    [Header("Ground Check Settings:")]
    [SerializeField] private Transform checkGroundPoint; // Diem kiem tra dat
    [SerializeField] private float checkGroundX = 0.2f; // Khoang cach kiem tra dat theo truc x
    [SerializeField] private float checkGroundY = 0.5f; // Khoang cach kiem tra dat theo truc y
    [SerializeField] private LayerMask groundCheck; // Lop de kiem tra dat
    [Space(5)]

    [Header("Jump Settings")]
    [SerializeField] private float addForce = 14f; // Luc nhay
    private int jumpBufferCounter = 0; // Dem thoi gian cho phep nhay
    [SerializeField] private int jumpBufferFrames = 10; // So frame cho phep nhay
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJump;
    [Space(5)]

    [Header("Dash Settings:")]
    [SerializeField] private float speedDash;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCoolDown;
    private bool canDash = true;
    private bool dashed;
    [Space(5)]

    [Header("Effect Settings:")]
    [SerializeField] private GameObject dashEffect;
    [SerializeField] private GameObject jumpEffect;
    [Space(5)]

    [Header("Attack Settings:")]
    private bool attack = false;
    private float timeBetweenAttack;
    private float timeSinceAttack;
    [SerializeField] private Transform upAttackTransform;
    [SerializeField] private Transform sideAttackTransform;
    [SerializeField] private Transform downAttackTransform;
    [SerializeField] private Vector2 upAttackArea;
    [SerializeField] private Vector2 sideAttackArea;
    [SerializeField] private Vector2 downAttackArea;
    [SerializeField] private LayerMask attackLayer;
    [SerializeField] private float damage;
    [Space(5)]

    // Singleton
    public static PlayerController instance;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Huy doi tuong neu da ton tai instance khac
        }
        else
        {
            instance = this; // Gan instance hien tai
        }
    }

    private PlayStateList pState;
    private Rigidbody2D rb;
    private Animator anim;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Lay component Rigidbody2D
        anim = GetComponent<Animator>(); // Lay component Animator
        pState = GetComponent<PlayStateList>(); // Lay component PlayStateList
        gravity = rb.gravityScale;
    }

    private void Update()
    {
        GetInputMove(); // Lay du lieu tu ban phim
        GetInputAttack();
        UpdateJump(); // Cap nhat trang thai nhay
        if (pState.dashing) return;
        Flip(); // Lat huong nhan vat
        Move(); // Di chuyen
        Jump(); // Thuc hien nhay
        StartDash();
        Attack();
    }

    //Input
    private void GetInputMove()
    {
        yAxis = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            xAxis = -1; // Di chuyen sang trai
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            xAxis = 1; // Di chuyen sang phai
        }
        else
        {
            xAxis = 0; // Dung lai
        }
    }
    private void GetInputAttack()
    {
        attack = Input.GetMouseButton(0);
    }

    // Player Move
    private void Move()
    {
        rb.velocity = new Vector2(xAxis * moveSpeed, rb.velocity.y); // Cap nhat toc do di chuyen
        anim.SetBool("Running", rb.velocity.x != 0 && Grounded()); // Cap nhat hoat hinh chay
    }

    //Player Flip
    private void Flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1); // Lat huong sang trai
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // Lat huong sang phai
        }
    }

    //Check Ground
    public bool Grounded()
    {
        // Kiem tra xem nhan vat co dang dung tren mat dat hay khong
        if (Physics2D.Raycast(checkGroundPoint.position, Vector2.down, checkGroundY, groundCheck)
            || Physics2D.Raycast(checkGroundPoint.position + new Vector3(-checkGroundX, 0, 0), Vector2.down, checkGroundY, groundCheck)
            || Physics2D.Raycast(checkGroundPoint.position + new Vector3(checkGroundX, 0, 0), Vector2.down, checkGroundY, groundCheck))
        {
            return true; // Nhan vat dang dung tren dat
        }
        else
        {
            return false; // Nhan vat khong dung tren dat
        }
    }

    //Jump
    private void Jump()
    {
        if ((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow)) && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            pState.jumping = false;
        }

        if (!pState.jumping)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, addForce);

                pState.jumping = true;

                jumpBufferCounter = 0;
                if (Grounded())
                {
                    Instantiate(jumpEffect, transform);
                }
            }
            else if (!Grounded() && airJumpCounter < maxAirJump && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
            {
                pState.jumping = true;

                airJumpCounter++;

                rb.velocity = new Vector3(rb.velocity.x, addForce);
            }
        }
        anim.SetBool("Jumping", !Grounded());
        
    }

    private void UpdateJump()
    {
        if (Grounded())
        {
            pState.jumping = false;

            coyoteTimeCounter = coyoteTime;

            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }

    //Dash
    private void StartDash()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift) && canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
        }
        if (Grounded())
        {
            dashed = false;
        }
    }
    private IEnumerator Dash()
    {
        canDash = false;
        pState.dashing = true;
        anim.SetTrigger("Dashing");
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * speedDash, 0);
        if (Grounded()) Instantiate(dashEffect, transform);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCoolDown);
        canDash = true;
    }
    //Gizmos Attack
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(sideAttackTransform.position, sideAttackArea);
        Gizmos.DrawWireCube(upAttackTransform.position, upAttackArea);
        Gizmos.DrawWireCube(downAttackTransform.position, downAttackArea);
    }
    //Attack
    private void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if(attack & timeSinceAttack > timeBetweenAttack)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");
            if(yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(sideAttackTransform, sideAttackArea);
            }
            else if(yAxis > 0)
            {
                Hit(upAttackTransform, upAttackArea);
            }
            else if(yAxis < 0 && !Grounded())
            {
                Hit(downAttackTransform, downAttackArea);
            }
        }
    }
    //Hit
    private void Hit(Transform attackTransform, Vector2 attackArea)
    {
        Collider2D[] objectToHit = Physics2D.OverlapBoxAll(attackTransform.position, attackArea, 0, attackLayer);
        if (objectToHit.Length > 0)
        {
            Debug.Log("Hit");
        }
        for (int i = 0; i < objectToHit.Length; i++)
        {
            if (objectToHit[i].GetComponent<EnemyController>() != null)
            {
                objectToHit[i].GetComponent<EnemyController>().EnemyHit(damage);
            }
        }
    }

}
