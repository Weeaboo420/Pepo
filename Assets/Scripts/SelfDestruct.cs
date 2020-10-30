using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public void Die()
    {
        Destroy(this.gameObject);
    }
}
