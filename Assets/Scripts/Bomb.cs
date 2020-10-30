using UnityEngine;

public class Bomb : MonoBehaviour
{
    private GameManager _gameManagerReference;

    private void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.075f);
        _gameManagerReference = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Skeleton"))
        {
            collision.GetComponent<Skeleton>().TakeDamage(1000);
            _gameManagerReference.CreateExplosionPrefab(new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f));
            _gameManagerReference.IncreaseBombPoints();
            Destroy(this.gameObject);
        }
    }
}
