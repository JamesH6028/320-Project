using UnityEngine;

public class CheeseInteraction : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player Component")
        {
            gameManager.IncrementCheeseCount();
            Destroy(transform.parent.gameObject);
        }
    }
}
