using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target; // A quién seguimos (El Player)

    [Header("Configuración")]
    public Vector3 offset = new Vector3(0, 10, -8); // La posición relativa (Arriba y atrás)
    public float smoothSpeed = 5f; // Que tan suave sigue al jugador (0 = duro, 10 = rápido)

    // LateUpdate se ejecuta DESPUÉS de que el jugador se haya movido.
    // Esto evita vibraciones raras.
    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calculamos dónde debería estar la cámara (Posición del jugador + Offset)
        Vector3 desiredPosition = target.position + offset;

        // 2. Nos movemos suavemente hacia allá (Interpolación Lineal - Lerp)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // 3. (Opcional) Hacemos que la cámara mire siempre al jugador
        // O puedes dejarla con rotación fija si prefieres una isométrica estricta.
        transform.LookAt(target.position);
    }
}