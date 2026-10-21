using UnityEngine;

public class CheeseInteraction : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player Component")
        {
            Destroy(transform.parent.gameObject);
        }
    }
}
