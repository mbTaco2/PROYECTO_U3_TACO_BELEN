using UnityEngine;

public class Candy : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EnemyAI enemy = FindObjectOfType<EnemyAI>();
            if (enemy != null)
            {
                enemy.CandyCollected();
                enemy.RemoveCandyFromList(transform); // Elimina el caramelo de la lista
            }

            Debug.Log("🍬 Dulce recogido por el niño.");
            Destroy(gameObject); // Elimina el dulce sin errores
        }
    }
}
