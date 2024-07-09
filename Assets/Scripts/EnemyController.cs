using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Recoil")]
    [SerializeField] private float recoilLength;
    [SerializeField] private float recoilFactor;
    [SerializeField] bool isRecoling = false;
    private float recoilTimer;
    [Space(5)]

    private Rigidbody2D rb;
    [SerializeField] private float health;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
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
    public void EnemyHit(float damage, Vector2 hitDirection, float hitForce)
    {
        health -= damage;
        if(!isRecoling )
        {
            rb.AddForce(-hitForce * recoilFactor * hitDirection);
        }
    }
}
