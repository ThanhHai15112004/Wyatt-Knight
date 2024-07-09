using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float health;
    private void Start()
    {
        
    }
    private void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }
    public void EnemyHit(float damage)
    {
        health -= damage;
    }
}
