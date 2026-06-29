using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;

    [Header("Limites horizontales")]
    public float limiteIzquierdo = -10f;
    public float limiteDerecho   =  10f;

    /// <summary>
    /// Bloqueo global de movimiento. Activarlo desde cualquier sistema
    /// (SceneLoader, cutscenes, etc.) inmoviliza a Mateo completamente.
    /// </summary>
    public static bool Bloqueado { get; set; } = false;

    private Rigidbody2D rb;
    private Animator    animator;
    private float       inputHorizontal;

    private void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        bool bloqueado = Bloqueado
                      || (DialogueManager.Instance != null &&
                         (DialogueManager.Instance.EstaActivo || DialogueManager.Instance.InputBloqueado))
                      || (EscenaRolManager.Instance  != null && EscenaRolManager.Instance.EstaActivo)
                      || (ReflexionManager.Instance  != null && ReflexionManager.Instance.EstaActivo);

        if (bloqueado)
        {
            inputHorizontal = 0f;
            animator.SetBool("isWalking", false);
            return;
        }

        inputHorizontal = Input.GetAxisRaw("Horizontal");

        bool estaCaminando = inputHorizontal != 0;
        animator.SetBool("isWalking", estaCaminando);

        if (inputHorizontal > 0)
            animator.SetInteger("lookDirection",  1);
        else if (inputHorizontal < 0)
            animator.SetInteger("lookDirection", -1);
    }

    private void FixedUpdate()
    {
        bool enLimiteIzquierdo = transform.position.x <= limiteIzquierdo && inputHorizontal < 0;
        bool enLimiteDerecho   = transform.position.x >= limiteDerecho   && inputHorizontal > 0;
        bool enLimite          = enLimiteIzquierdo || enLimiteDerecho;

        rb.linearVelocity = enLimite
            ? Vector2.zero
            : new Vector2(inputHorizontal * velocidad, 0f);

        float xClamp = Mathf.Clamp(transform.position.x, limiteIzquierdo, limiteDerecho);
        transform.position = new Vector3(xClamp, transform.position.y, transform.position.z);

        if (enLimite)
            animator.SetBool("isWalking", false);
    }
}