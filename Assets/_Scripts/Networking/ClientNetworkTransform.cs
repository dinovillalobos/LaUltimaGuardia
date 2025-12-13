using Unity.Netcode.Components;
using UnityEngine;

// Heredamos de NetworkTransform para cambiarle una sola regla
public class ClientNetworkTransform : NetworkTransform
{
    // Sobrescribimos la regla de "Quién manda".
    // false = El servidor NO manda sobre este objeto. El dueño (Cliente) manda.
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}