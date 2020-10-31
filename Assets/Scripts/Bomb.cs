using UnityEngine;

public class Bomb : MonoBehaviour
{
    private GameManager _gameManagerReference;
    private bool _hasExploded = false;

    private void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.075f);
        _gameManagerReference = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Skeleton") && collision.isTrigger)
        {
            if(!_hasExploded)
            {
                _hasExploded = true;
                _gameManagerReference.IncreaseBombPoints();
            }

            collision.GetComponent<Skeleton>().TakeDamage(150);
            _gameManagerReference.CreateExplosionPrefab(new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f));            
            Destroy(this.gameObject);
        }
    }
}
