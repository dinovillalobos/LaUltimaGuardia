using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    private CharacterController _cc;
    private GameInput _input; // La clase generada por el Input System
    private Vector2 _moveInput;

    // Inicialización del Input
    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _input = new GameInput();

        // Lambda para leer el valor cuando cambia
        _input.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _input.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
    }

    // Activar/Desactivar inputs al entrar/salir
    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    public override void OnNetworkSpawn()
    {
        // Esto es CRÍTICO:
        // Si este objeto NO es mío (es el personaje de otro jugador conectado),
        // desactivo su input para no controlarlo yo por error.
        if (!IsOwner)
        {
            _input.Disable();
            return;
        }

        // Si SOY el dueño, muevo la cámara para que me siga a mí
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0, 10, -8); // Vista Top-Down
        Camera.main.transform.localRotation = Quaternion.Euler(60, 0, 0);
    }

    void Update()
    {
        // Seguridad doble: Solo el dueño ejecuta el movimiento
        if (!IsOwner) return;

        Mover();
        Rotar();
    }

    void Mover()
    {
        // Convertir input 2D (XY) a movimiento 3D (XZ)
        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;

        // Gravedad simple (para que no flote)
        Vector3 velocity = direction * moveSpeed;
        if (!_cc.isGrounded) velocity.y = -9.81f;

        _cc.Move(velocity * Time.deltaTime);
    }

    void Rotar()
    {
        if (_moveInput == Vector2.zero) return;

        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y);
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}