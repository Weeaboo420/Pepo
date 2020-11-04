using System.Collections.Generic;
using UnityEngine;

public class Puddle : MonoBehaviour
{
    private List<Skeleton> _affectedSkeletons = new List<Skeleton>();    

    private void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.075f + 9);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Skeleton"))
        {
            Skeleton skeletonScript = collision.GetComponent<Skeleton>();
            _affectedSkeletons.Add(skeletonScript);
            skeletonScript.AddSlowedEffect();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Skeleton"))
        {
            Skeleton skeletonScript = collision.GetComponent<Skeleton>();
            _affectedSkeletons.Remove(skeletonScript);
            skeletonScript.RemoveSlowedEffect();
        }
    }

    public void Die()
    {
        foreach(Skeleton skeletonScript in _affectedSkeletons)
        {
            skeletonScript.RemoveSlowedEffect();
        }

        Destroy(this.gameObject);
    }
}
