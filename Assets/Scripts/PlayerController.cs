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
    [SerializeField] private float timeBetweenAttack = 1f;
    [SerializeField] private Transform upAttackTransform;
    [SerializeField] private Transform sideAttackTransform;
    [SerializeField] private Transform downAttackTransform;
    [SerializeField] private Vector2 upAttackArea;
    [SerializeField] private Vector2 sideAttackArea;
    [SerializeField] private Vector2 downAttackArea;
    [SerializeField] private LayerMask attackLayer;
    [SerializeField] private float damage;
    private bool attack = false;
    private float timeSinceAttack = 0f;
    [Space(5)]

    [Header("Slash Effect")]
    [SerializeField] private GameObject slashObject;
    [Space(5)]

    [Header("Recoil")]
    [SerializeField] private int recoilXSteps = 5;
    [SerializeField] private int recoilYSteps = 5;
    [SerializeField] private float recoilXSpeed = 100;
    [SerializeField] private float recoilYSpeed = 100;
    private int stepsXRecoiled;
    private int stepsYRecoiled;
    [Space(5)]

    [Header("Health Setting")]
    public int health;
    public int maxHealth;
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
        health = maxHealth;
    }


    [HideInInspector] public PlayStateList pState;
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
        Recoil();
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
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector3(1, 1, 1); // Lat huong sang phai
            pState.lookingRight = true;
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
                rb.velocity = new Vector2(rb.velocity.x, addForce);

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

                rb.velocity = new Vector2(rb.velocity.x, addForce);
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
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !dashed)
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
        if (attack & timeSinceAttack > timeBetweenAttack)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");
            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(sideAttackTransform, sideAttackArea, ref pState.recoilX, recoilXSpeed);
                Instantiate(slashObject, sideAttackTransform);
            }
            else if (yAxis > 0)
            {
                Hit(upAttackTransform, upAttackArea, ref pState.recoilY, recoilYSpeed);
                SlashEffect(slashObject, 90, upAttackTransform);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(downAttackTransform, downAttackArea, ref pState.recoilY, recoilYSpeed);
                SlashEffect(slashObject, -90, downAttackTransform);
            }
        }
    }
    //Hit
    private void Hit(Transform attackTransform, Vector2 attackArea, ref bool recoilDir, float recoilStrength)
    {
        Collider2D[] objectToHit = Physics2D.OverlapBoxAll(attackTransform.position, attackArea, 0, attackLayer);
        if (objectToHit.Length > 0)
        {
            recoilDir = true;
        }
        for (int i = 0; i < objectToHit.Length; i++)
        {
            if (objectToHit[i].GetComponent<EnemyController>() != null)
            {
                objectToHit[i].GetComponent<EnemyController>().EnemyHit(damage,
                    (transform.position - objectToHit[i].transform.position).normalized, recoilStrength);
            }
        }
    }
    //TakeDamage
    public void TakeDamage(float damage)
    {
        health -= Mathf.RoundToInt(damage);
        StartCoroutine(StopTakingDamage());
    }
    private IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        anim.SetTrigger("Hurting");
        ClampHealth();
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }
    //SlashEffect
    private void SlashEffect(GameObject slashEffect, int effectAngle, Transform attackTransform)
    {
        slashEffect = Instantiate(slashEffect, attackTransform);
        slashEffect.transform.eulerAngles = new Vector3 (0, 0, effectAngle);
        slashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }
    //Stop recoil
    private void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilX = false;
    }
    private void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilY = false;
    }
    //Recoil
    private void Recoil()
    {
        if (pState.recoilX)
        {
            if (pState.recoilX)
            {
                if (pState.lookingRight)
                {
                    rb.velocity = new Vector2(-recoilXSpeed, 0);
                }
                else
                {
                    rb.velocity = new Vector2(recoilXSpeed, 0);
                }
            }
        }
        if (pState.recoilY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
            airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }
        //stop recoil
        if(pState.recoilX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }
        if (pState.recoilY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }
        if (Grounded())
        {
            StopRecoilY();
        }
    }
    //Health
    private void ClampHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
    }
}   
