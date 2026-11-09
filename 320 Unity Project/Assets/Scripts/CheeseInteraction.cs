using UnityEngine;

public class CheeseInteraction : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player Component")
        {
            audioSource.Play();
            gameManager.IncrementCheeseCount();
            Destroy(transform.parent.gameObject);
        }
    }
}
