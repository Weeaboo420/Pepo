using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    private GameManager _gameManagerReference;
    private int _hits = 0;
    private const int _maxHits = 3;
    private const float _explosionRadius = 3.9f; //The radius of the actual explosion
    private const float _triggerRadius = 1.5f;  //The radius of the area around the bomb that will trigger an explosion
    private const int _damage = 130;

    private void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.075f);
        _gameManagerReference = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(_triggerRadius, _triggerRadius), 0f, Vector2.zero);

        if (hits.Length > 0)
        {
            
            bool willExplode = false;

            foreach (RaycastHit2D hit in hits)
            {
                if(hit.transform.CompareTag("Skeleton") && hit.collider.isTrigger)
                {
                    //We only need to determine if a skeleton has entered the trigger radius
                    willExplode = true;
                    break;
                }
            }

            if (willExplode)
            {
                List<Skeleton> hitSkeletons = new List<Skeleton>();

                RaycastHit2D[] explosionHits = Physics2D.BoxCastAll(transform.position, new Vector2(_explosionRadius, _explosionRadius), 0f, Vector2.zero);
                foreach(RaycastHit2D hit in explosionHits)
                {
                    //Make sure that we only hit objects tagged with "Skeleton"
                    if(hit.transform.CompareTag("Skeleton") && hit.collider.isTrigger && _hits < _maxHits)
                    {
                        //If there is a direct line of sight between the bomb and the skeleton
                        if(Physics2D.Raycast(transform.position, (hit.transform.position - transform.position), _explosionRadius, LayerMask.GetMask("Skeleton")))
                        {
                            hitSkeletons.Add(hit.transform.GetComponent<Skeleton>());
                            _hits++;
                        }
                    }

                    if(_hits == _maxHits)
                    {
                        break;
                    }
                }

                //Damage all the skeletons in range
                foreach(Skeleton skeletonScript in hitSkeletons)
                {
                    skeletonScript.TakeDamage(_damage);
                }

                _gameManagerReference.CreateExplosionPrefab(new Vector3(transform.position.x, transform.position.y, transform.position.z - 1f));
                Destroy(this.gameObject);
            }
        }
    }
}
