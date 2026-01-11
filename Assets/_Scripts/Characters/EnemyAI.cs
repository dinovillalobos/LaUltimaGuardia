using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : NetworkBehaviour
{
    [Header("Objetivo")]
    private Transform target; // La Reliquia o el Jugador

    private NavMeshAgent _agent;
    private float _updatePathTimer; // Para no calcular ruta cada frame (optimización)

    public override void OnNetworkSpawn()
    {
        // La IA solo se calcula en el SERVIDOR.
        // Los clientes solo ven el resultado (la posición sincronizada por NetworkTransform).
        if (!IsServer)
        {
            enabled = false; // Apagamos este script en los clientes para ahorrar recursos
            return;
        }

        _agent = GetComponent<NavMeshAgent>();

        // Buscar la Reliquia al nacer
        GameObject relicObj = GameObject.Find("Relic");
        if (relicObj != null)
        {
            target = relicObj.transform;
        }
    }

    void Update()
    {
        if (!IsServer || target == null) return;

        // Calculamos la ruta cada 0.5 segundos, no cada frame (Ahorra CPU)
        _updatePathTimer += Time.deltaTime;
        if (_updatePathTimer > 0.5f)
        {
            _updatePathTimer = 0;
            _agent.SetDestination(target.position);
        }

        // Aquí luego pondremos la lógica de ataque
        // Si distancia < 1.5f -> Atacar()
    }
}