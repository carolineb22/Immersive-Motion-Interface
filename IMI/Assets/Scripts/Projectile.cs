using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Transform target;
    public float speed = 10f;
    public int damage;
    public BattleManager battleManager;

    void Update()
    {
        if (target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // When it reaches target
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        Unit unit = target.GetComponent<Unit>();

        if (unit != null)
        {
            unit.TakeDamage(damage);

            if (unit.IsDead())
            {
                battleManager.battleLog.Log(unit.unitName + " Enemy was defeated!");
                battleManager.OnEnemyDefeated();
            }
            else
            {
                battleManager.StartCoroutine(battleManager.DelayedEnemyTurn());
            }
        }

        Destroy(gameObject);
    }
}