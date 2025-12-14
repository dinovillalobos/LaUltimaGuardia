using Unity.Netcode;
using UnityEngine;

public class NetworkHealth : NetworkBehaviour, IDamageable
{
    // NetworkVariable: Variable mágica que se sincroniza sola.
    // readPerm: Everyone (Todos pueden ver la vida del otro)
    // writePerm: Server (Solo el server puede bajar vida)
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // Suscribirse al cambio de valor (útil para actualizar barras de vida UI luego)
        CurrentHealth.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentHealth.OnValueChanged -= OnHealthChanged;
    }

    // Implementación de la Interfaz
    public void TakeDamage(int amount)
    {
        // Seguridad: Solo el servidor puede ejecutar esto
        if (!IsServer) return;

        CurrentHealth.Value -= amount;

        if (CurrentHealth.Value <= 0)
        {
            CurrentHealth.Value = 0;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(name + " ha muerto.");
        // Aquí luego pondremos lógica de respawn o game over
        if (IsServer) GetComponent<NetworkObject>().Despawn();
    }

    private void OnHealthChanged(int previous, int current)
    {
        // Solo para debug visual por ahora
        Debug.Log($"Salud de {name}: {previous} -> {current}");
    }
}