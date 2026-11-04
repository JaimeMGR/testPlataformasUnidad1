
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 2f;

    [Header("Detección")]
    [SerializeField] private float distanciaPared = 0.15f;
    [SerializeField] private float distanciaSuelo = 0.3f;
    [SerializeField] private LayerMask capaSuelo;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sprite;

    // 1 = derecha, -1 = izquierda
    private int direccion = 1;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();

        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        ComprobarEntorno();

        // Movimiento horizontal
        rb.linearVelocity = new Vector2(
            direccion * velocidad,
            rb.linearVelocity.y
        );

        // Girar el sprite
        if (sprite != null)
            sprite.flipX = direccion < 0;
    }

    private void ComprobarEntorno()
    {
        Bounds limites = col.bounds;

        // Detectar pared delante del enemigo
        Vector2 origenPared = new Vector2(
            direccion > 0 ? limites.max.x : limites.min.x,
            limites.center.y
        );

        RaycastHit2D pared = Physics2D.Raycast(
            origenPared,
            Vector2.right * direccion,
            distanciaPared,
            capaSuelo
        );

        // Detectar si hay suelo delante
        Vector2 origenSuelo = new Vector2(
            direccion > 0 ? limites.max.x + 0.05f : limites.min.x - 0.05f,
            limites.min.y + 0.05f
        );

        RaycastHit2D suelo = Physics2D.Raycast(
            origenSuelo,
            Vector2.down,
            distanciaSuelo,
            capaSuelo
        );

        // Si hay una pared o no hay suelo, cambiar dirección
        if (pared.collider != null || suelo.collider == null)
        {
            direccion *= -1;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (col == null) return;

        Bounds limites = col.bounds;

        Vector2 origenSuelo = new Vector2(
            direccion > 0 ? limites.max.x + 0.05f : limites.min.x - 0.05f,
            limites.min.y + 0.05f
        );

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            origenSuelo,
            origenSuelo + Vector2.down * distanciaSuelo
        );
    }
}
