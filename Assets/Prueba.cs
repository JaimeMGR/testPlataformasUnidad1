using UnityEngine;

public class Prueba : MonoBehaviour
{
    private Rigidbody2D rb;
    private float movimientoX;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        movimientoX = Input.GetAxis("Horizontal");
        //movimientoY = Input.GetAxis("Vertical");

        rb.AddForce(new Vector2(movimientoX * 10, 0));



    }
}
