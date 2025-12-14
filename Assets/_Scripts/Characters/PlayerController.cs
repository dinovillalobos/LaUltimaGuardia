using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 3f; // Velocidad de giro

    // Necesitamos definir qué capas son "Suelo" para que el rayo choque ahí
    [SerializeField] private LayerMask groundLayer;

    private CharacterController _cc;
    private GameInput _input;
    private Vector2 _moveInput;
    private Camera _mainCamera;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _input = new GameInput();
        _input.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _input.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
    }

    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            _input.Disable();
            return;
        }

        // CORRECCIÓN AQUÍ: Asignamos a la variable global _mainCamera
        _mainCamera = Camera.main;

        // A partir de aquí usamos _mainCamera en lugar de mainCam
        if (_mainCamera != null)
        {
            // Le añadimos el script de seguimiento si no lo tiene
            CameraFollow camScript = _mainCamera.GetComponent<CameraFollow>();
            if (camScript == null) camScript = _mainCamera.gameObject.AddComponent<CameraFollow>();

            // LE DECIMOS QUE ME SIGA A MÍ
            camScript.target = this.transform;

            // Configuramos el ángulo
            camScript.offset = new Vector3(0, 12, -7);
        }
        else
        {
            Debug.LogError("¡No encontré una Cámara etiquetada como MainCamera en la escena!");
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        RotarHaciaElMouse();
        Mover();
    }

    void RotarHaciaElMouse()
    {
        // 1. Posición del mouse
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();

        // 2. Raycast
        Ray ray = _mainCamera.ScreenPointToRay(mouseScreenPosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, groundLayer))
        {
            Vector3 targetPoint = hitInfo.point;
            targetPoint.y = transform.position.y;

            // --- CAMBIO AQUÍ ---
            // En lugar de mirar instantáneamente, calculamos la dirección
            Vector3 directionToLook = targetPoint - transform.position;

            // Evitamos errores si la dirección es cero (el mouse está justo encima del personaje)
            if (directionToLook != Vector3.zero)
            {
                // Calculamos la rotación deseada
                Quaternion targetRotation = Quaternion.LookRotation(directionToLook);

                // Rotamos suavemente desde la rotación actual hacia la deseada
                // 'turnSpeed' controla qué tan rápido gira.
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }
    }

    void Mover()
    {
        if (_moveInput == Vector2.zero) return;

        // --- CAMBIO RADICAL ---

        // Antes (Estilo Zomboid):
        // Vector3 direction = transform.forward * _moveInput.y + transform.right * _moveInput.x;

        // AHORA (Estilo Hades / Twin Stick):
        // El movimiento es absoluto respecto al mundo/cámara.
        // W siempre es +Z (Norte), D siempre es +X (Este).
        // No importa si tu personaje está mirando a China, si aprietas W, va para arriba.

        Vector3 direction = new Vector3(_moveInput.x, 0, _moveInput.y).normalized;

        // Si tu cámara está rotada (ej. 45 grados), usa este bloque en su lugar:
        /*
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();
        direction = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;
        */

        Vector3 velocity = direction * moveSpeed;
        if (!_cc.isGrounded) velocity.y = -9.81f;

        _cc.Move(velocity * Time.deltaTime);
    }
}