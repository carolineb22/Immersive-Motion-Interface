using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;

    void Update()
    {
        if (target == null) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        Debug.Log("Hit!");

        //will add hit effects here later

        Destroy(gameObject);
    }
}