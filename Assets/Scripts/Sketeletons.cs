using UnityEngine;

public class Sketeletons : EnemyController
{
    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
       rb.gravityScale = 7f;
    }
    protected override void Update()
    {
        base.Update();
        if(!isRecoling)
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2
                (PlayerController.instance.transform.position.x, transform.position.y), speed * Time.deltaTime);
        }
    }
    public override void EnemyHit(float damage, Vector2 hitDirection, float hitForce)
    {
        base.EnemyHit(damage, hitDirection, hitForce);
    }
}
