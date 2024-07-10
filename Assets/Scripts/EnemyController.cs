using UnityEngine;

public abstract class EnemyController : MonoBehaviour
{
    [Header("Recoil")]
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoling = false;
    protected float recoilTimer;
    [Space(5)]

    [Header("Enemy AI")]
    [SerializeField] protected float health;
    [SerializeField] protected float speed;
    [SerializeField] protected float damage;
    [Space(5)]

    protected PlayerController playerController;
    protected Rigidbody2D rb;
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = PlayerController.instance;
    }
    protected virtual void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }
        if(isRecoling)
        {
            if(recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoling = false;
                recoilTimer = 0;
            }
        }
    }
    public virtual void EnemyHit(float damage, Vector2 hitDirection, float hitForce)
    {
        health -= damage;
        if(!isRecoling )
        {
            rb.AddForce(-hitForce * recoilFactor * hitDirection);
        }
    }
    protected virtual void Attack()
    {
        PlayerController.instance.TakeDamage(damage);
    }
    protected void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !PlayerController.instance.pState.invincible)
        {
            Attack();
        }
    }
}
