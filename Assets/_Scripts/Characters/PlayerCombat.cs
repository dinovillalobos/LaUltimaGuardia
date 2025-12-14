using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask enemyLayer; // Capa de lo que se puede golpear

    private GameInput _input;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _input = new GameInput();
        _input.Player.Attack.performed += ctx => Attack();
        _input.Enable();
    }

    private void Attack()
    {
        // 1. Lógica Visual (Animación) - Cliente
        Debug.Log("¡Swing de espada!");

        // 2. Lógica Real - Pedir al servidor que calcule el golpe
        RequestAttackServerRpc();
    }

    [ServerRpc]
    private void RequestAttackServerRpc()
    {
        // ESTO OCURRE EN EL SERVIDOR

        // Simulación simple: Esfera invisible frente al jugador
        // Detecta todo lo que esté en frente y tenga la capa "enemyLayer" o "Default"
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward, attackRange);

        foreach (var hit in hits)
        {
            // Evitar golpearme a mí mismo
            if (hit.gameObject == gameObject) continue;

            // Preguntar si tiene vida
            if (hit.TryGetComponent<IDamageable>(out IDamageable target))
            {
                target.TakeDamage(damage);
                Debug.Log($"Golpeaste a {hit.name} en el Servidor");
            }
        }
    }

    // Dibujar la esfera en el editor para ver el rango
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, attackRange);
    }
}