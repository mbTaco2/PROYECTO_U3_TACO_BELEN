using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // Estados del enemigo: Patrullando o Persiguiendo
    public enum BossState { Patrolling, Chasing }
    private BossState currentState = BossState.Patrolling; // Estado inicial: patrulla

   
    public float speed = 2f;           // Velocidad de patrullaje
    public float chaseSpeed = 8.5f;    // Velocidad al perseguir al jugador
    public float gravity = -9.8f;      // Gravedad aplicada al enemigo
    public Transform[] waypoints;      // Lista de waypoints para patrullaje

  
    public Transform player;           // Referencia al jugador (niño)
    public Transform[] candies;        // Referencia a los dulces en la escena
    public float detectionRange = 3.5f;// Distancia mínima para que el niño active la persecución
    public float catchDistance = 1.5f; // Distancia mínima para atrapar al niño
    public LayerMask obstacleLayer;    // Capa de obstáculos en el mapa

    private CharacterController characterController; // Controlador de movimiento
    private Vector3 velocity;                        // Vector de velocidad
    private int currentWaypointIndex = 0;            // Índice del waypoint actual
    private Animator animator;                       // Controlador de animaciones
    private int candiesCollected = 0;                // Contador de caramelos recogidos

    void Start()
    {
        characterController = GetComponent<CharacterController>(); // Obtiene el CharacterController
        animator = GetComponent<Animator>(); // Obtiene el Animator para animaciones
    }

    void Update()
    {
        ApplyGravity(); // Aplica gravedad en cada frame

        // Estado Finito de la IA
        switch (currentState)
        {
            case BossState.Patrolling:
                Patrullaje(); // Patrulla usando waypoints
                if (IsNearCandy()) // Si el niño se acerca a un dulce, inicia la persecución
                {
                    Debug.Log("🔥 Niño se acercó a un dulce, MAMÁ ENTRA EN MODO PERSECUCIÓN.");
                    currentState = BossState.Chasing; // Cambia de patrullaje a persecución
                    animator.SetFloat("Speed", chaseSpeed); // Activa animación de correr
                }
                break;

            case BossState.Chasing:
                ChasePlayer(); // Persigue al jugador
                break;
        }

        // Si el niño recoge todos los dulces, gana
        if (candiesCollected == 3)
        {
            Debug.Log("✅ El niño recogió todos los caramelos y ganó.");
            EndGame(true); // Llama a EndGame() indicando que el niño ganó
        }
    }

    // IA de patrullaje usando waypoints
    void Patrullaje()
    {
        if (waypoints.Length == 0) return; // Si no hay waypoints, no hacer nada

        Transform targetWaypoint = waypoints[currentWaypointIndex]; // Waypoint objetivo
        MoveTowards(targetWaypoint.position, speed); // Se mueve hacia el waypoint
        Debug.Log($"🚶 Patrullando hacia: {waypoints[currentWaypointIndex].name}");

        // Si llega al waypoint, selecciona el siguiente
        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < 1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    // IA de persecución con evasión de obstáculos
    void ChasePlayer()
    {
        if (!PathBlocked(player.position)) // Si no hay obstáculos, va directo al jugador
        {
            Debug.Log($"🚀 Persiguiendo directamente al niño.");
            MoveTowards(player.position, chaseSpeed);
        }
        else // Si hay obstáculos, busca el mejor camino usando waypoints
        {
            Debug.Log("⛔ Obstáculo detectado, buscando alternativa.");
            Transform bestWaypoint = FindBestPathToPlayer();
            if (bestWaypoint != null)
            {
                Debug.Log($"⚡ Esquivando obstáculos, yendo a {bestWaypoint.name}");
                MoveTowards(bestWaypoint.position, chaseSpeed);
            }
            else
            {
                Debug.LogWarning("❌ No hay waypoints accesibles. Intentando moverse libremente.");
                TryLateralEscape(); // Si no hay waypoints libres, intenta moverse lateralmente
            }
        }

        // Si la mamá alcanza al niño, lo atrapa y finaliza el juego
        if (Vector3.Distance(transform.position, player.position) < catchDistance)
        {
            Debug.Log("❌ Mamá atrapó al niño. Fin del juego.");
            animator.SetTrigger("isAttacking"); // Activa la animación de ataque
            EndGame(false); // El juego termina con derrota del niño
        }
    }

    // Intenta esquivar obstáculos moviéndose lateralmente
    void TryLateralEscape()
    {
        Vector3 left = transform.position + transform.right * -2f;
        Vector3 right = transform.position + transform.right * 2f;

        if (!PathBlocked(left))
        {
            Debug.Log("⬅ Mamá esquiva a la izquierda.");
            MoveTowards(left, chaseSpeed);
        }
        else if (!PathBlocked(right))
        {
            Debug.Log("➡ Mamá esquiva a la derecha.");
            MoveTowards(right, chaseSpeed);
        }
        else
        {
            Debug.Log("🔄 No hay salida, moviéndose aleatoriamente.");
            MoveTowards(transform.position + Random.insideUnitSphere * 2f, chaseSpeed);
        }
    }

    // Se mueve hacia un objetivo
    void MoveTowards(Vector3 target, float moveSpeed)
    {
        Vector3 direction = (target - transform.position).normalized;
        characterController.Move(direction * moveSpeed * Time.deltaTime);
        transform.LookAt(new Vector3(target.x, transform.position.y, target.z));

        animator.SetFloat("Speed", moveSpeed); // Controla la animación según la velocidad
    }

    // Detecta si el niño está cerca de un caramelo
    bool IsNearCandy()
    {
        foreach (Transform candy in candies)
        {
            float distance = Vector3.Distance(player.position, candy.position);
            if (distance < detectionRange)
            {
                return true;
            }
        }
        return false;
    }

    // Detecta si hay un obstáculo entre la mamá y un punto objetivo
    bool PathBlocked(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);
        return Physics.Raycast(transform.position, direction, distance, obstacleLayer);
    }

    // Algoritmo para encontrar el mejor camino al jugador usando waypoints
    Transform FindBestPathToPlayer()
    {
        List<Transform> orderedWaypoints = waypoints
            .OrderBy(waypoint => Vector3.Distance(waypoint.position, player.position))
            .ToList();

        foreach (Transform waypoint in orderedWaypoints)
        {
            if (!PathBlocked(waypoint.position))
            {
                return waypoint; // Devuelve el primer waypoint accesible
            }
        }

        return null; // Si no hay waypoints libres, regresa null
    }

    void ApplyGravity()
    {
        if (!characterController.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
        else
        {
            velocity.y = -2f;
        }
    }

    // Registra la recolección de caramelos
    public void CandyCollected()
    {
        candiesCollected++;
        Debug.Log($"🍬 Dulces recogidos: {candiesCollected}/{candies.Length}");
    }

    // Finaliza el juego
    void EndGame(bool playerWon)
    {
        Debug.Log(playerWon ? "🎉 ¡Felicidades! El niño ganó." : "❌ Mamá atrapó al niño. Fin del juego.");
        Time.timeScale = 0; // Pausa el juego
    }

public void RemoveCandyFromList(Transform candy)
    {
        candies = candies.Where(c => c != candy).ToArray(); // Elimina el caramelo de la lista
    }
}
