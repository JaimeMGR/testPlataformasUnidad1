using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerControllerNewMonoBehaviourScript : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float movimientoX = 5.0f;
    [SerializeField] private float movimientoY = 2f;
    [SerializeField] private float velocidadMovimiento = 6f;
    [SerializeField] private float aceleracionSuelo = 50f;
    [SerializeField] private float frenado = 60f;
    [SerializeField] private float controlAereo = 0.5f;

    [Header("Salto")]
    [SerializeField] private float fuerzaSalto = 8f;
    [SerializeField] private float fuerzaSaltoAdicional = 20f;
    [SerializeField] private float tiempoMaximoSalto = 0.25f;
    [SerializeField] private float gravedad = 3f;

    [Header("Interfaz")]
    [SerializeField] private TextMeshProUGUI panelPuntuación;
    [SerializeField] private TextMeshProUGUI panelTiempo;
    [SerializeField] private TextMeshProUGUI panelMonedas;
    [SerializeField] private TextMeshProUGUI panelTemporizadorDobleSalto;
    [SerializeField] private TextMeshProUGUI panelVidas;
    [SerializeField] private Slider barraVida;
    [SerializeField] private GameObject UI_jugador;
    [SerializeField] private ParticleSystem muerteFX;
    [SerializeField] private ParticleSystem dineroFX;

    [Header("Vida y vidas")]
    [SerializeField] private float vidaMaxima = 3f;
    [SerializeField] private float dañoPinchos = 1f;
    [SerializeField] private float dañoPinchosgrave = 99f;
    [SerializeField] private float dañoEnemigo = 1f;
    [SerializeField] private int vidas = 3;
    private float vidaActual;

    [Header("Invulnerabilidad")]
    [SerializeField] private float duracionInvulnerabilidad = 1.5f;
    [SerializeField] private float intervaloParpadeo = 0.1f;
    [SerializeField] private SpriteRenderer spriteJugador;
    private bool invulnerable = false;
    private float temporizadorInvulnerabilidad = 0f;
    private float temporizadorParpadeo = 0f;

    [Header("Colisiones")]
    [SerializeField] private Collider2D colliderJugador;

    // Spawnpoint
    [Header("Reaparición y sonidos")]
    [SerializeField] private Collider2D trigger;
    [SerializeField] private GameObject advertencia;
    [SerializeField] private AudioSource sonidoPinchos;
    [SerializeField] private AudioSource sonidoMoneda;
    [SerializeField] private AudioSource sonidoObtener;
    [SerializeField] private AudioSource musicaNivel;
    [SerializeField] private AudioSource musicaDerrota;

    [SerializeField] private GameObject pausa;
    [SerializeField] private GameObject keyUI;

    [SerializeField] private bool haskey = false;

    private Rigidbody2D rb;
    [SerializeField] private bool puedeSaltar;

    private int puntuacion = 0;
    private float tiempoTotal = 0f;
    private int monedas = 0;
    private float inputHorizontal;

    // Control del personaje durante la muerte
    private bool puedeMoverse = true;

    // Variables para el salto variable
    private bool manteniendoSalto;
    private bool saltoEnCurso;
    private float tiempoSalto;

    [Header("Objetos del nivel")]
    [SerializeField] private GameObject puerta;
    [SerializeField] private GameObject pantallaFin;
    [SerializeField] private GameObject pantallaMuerte;
    [SerializeField] private GameObject textopuerta;
    private Transform spawnActivo;

    [Header("Spawnpoints y checkpoints")]
    [SerializeField] private Transform spawnPoint; // SpawnPoint 1
    [SerializeField] private Transform SpawnPoint2; // SpawnPoint 2

    [SerializeField] private GameObject CheckPoint;  // Checkpoint 1
    [SerializeField] private GameObject CheckPoint2; // Checkpoint 2

    private Transform spawnActual;

    [Header("Doble salto")]
    [SerializeField] private int saltosMaximos = 2;
    private int saltosDisponibles;

    [Header("Retardo del doble salto")]
    [SerializeField] private float retardoDobleSalto = 2f;
    private float temporizadorDobleSalto = 0f;
    private bool puedeUsarDobleSalto = true;

    [Header("Enemigos")]
    [SerializeField] private float fuerzaReboteEnemigo = 8f;
    [SerializeField] private float margenPisoton = 0.2f;

    [SerializeField] private AudioSource sonidoEnemigo;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("El jugador necesita un componente Rigidbody2D.");
            enabled = false;
            return;
        }

        rb.gravityScale = gravedad;
        rb.freezeRotation = true;

        if (spriteJugador == null)
            spriteJugador = GetComponentInChildren<SpriteRenderer>();

        if (colliderJugador == null)
            colliderJugador = GetComponent<Collider2D>();

        vidaActual = vidaMaxima;
        ActualizarBarraVida();
        ActualizarPanelVidas();

        puedeSaltar = false;
        saltosDisponibles = saltosMaximos;
        puedeUsarDobleSalto = true;
        temporizadorDobleSalto = 0f;

        if (puerta != null)
            puerta.SetActive(true);
    }

    private void Update()
    {
        ActualizarRetardoDobleSalto();
        ActualizarInvulnerabilidad();

        // Leer el input únicamente si el personaje puede moverse.
        if (puedeMoverse)
        {
            inputHorizontal = Input.GetAxisRaw("Horizontal");

            if (Input.GetKeyDown(KeyCode.Space))
                IntentarSaltar();

            if (Input.GetKeyUp(KeyCode.Space))
            {
                manteniendoSalto = false;
                saltoEnCurso = false;
            }
            else if (Input.GetKey(KeyCode.Space) && saltoEnCurso)
            {
                manteniendoSalto = true;
            }
        }
        else
        {
            inputHorizontal = 0f;
            manteniendoSalto = false;
            saltoEnCurso = false;
        }

        // Controlar el tiempo total sin decimales.
        tiempoTotal = Mathf.FloorToInt(Time.time);
        if (panelTiempo != null)
            panelTiempo.text = "Tiempo: " + tiempoTotal.ToString();

        // Pausa.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1f)
            {
                pausar();
                Debug.Log("Se pausa el juego");
            }
            else
            {
                Time.timeScale = 1f;
                if (pausa != null)
                    pausa.SetActive(false);
            }
        }
    }

    private void FixedUpdate()
    {
        // Bloquear completamente el movimiento durante la muerte o reaparición.
        if (!puedeMoverse)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Movimiento horizontal.
        float velocidadObjetivo = inputHorizontal * velocidadMovimiento;
        float control = puedeSaltar ? 1f : controlAereo;
        float velocidadActualX = rb.linearVelocity.x;

        float aceleracion = Mathf.Abs(inputHorizontal) > 0.01f
            ? aceleracionSuelo * control
            : frenado;

        float nuevaVelocidadX = Mathf.MoveTowards(
            velocidadActualX,
            velocidadObjetivo,
            aceleracion * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(nuevaVelocidadX, rb.linearVelocity.y);

        // Salto variable: aplicar fuerza adicional mientras se mantiene espacio.
        if (saltoEnCurso && manteniendoSalto && rb.linearVelocity.y > 0f)
        {
            if (tiempoSalto < tiempoMaximoSalto)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y + fuerzaSaltoAdicional * Time.fixedDeltaTime
                );

                tiempoSalto += Time.fixedDeltaTime;
            }
            else
            {
                saltoEnCurso = false;
            }
        }

        // Dejar de aplicar fuerza adicional cuando empieza la caída.
        if (rb.linearVelocity.y <= 0f)
            saltoEnCurso = false;
    }

    private void IntentarSaltar()
    {
        bool esSaltoDesdeSuelo = puedeSaltar && saltosDisponibles > 0;
        bool esDobleSalto = !puedeSaltar
                            && saltosDisponibles > 0
                            && saltosDisponibles < saltosMaximos
                            && puedeUsarDobleSalto;

        if (!esSaltoDesdeSuelo && !esDobleSalto)
            return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
        saltosDisponibles--;

        if (esSaltoDesdeSuelo)
        {
            // El segundo salto queda bloqueado hasta que termine la cuenta atrás.
            temporizadorDobleSalto = Mathf.Max(0f, retardoDobleSalto);
            puedeUsarDobleSalto = temporizadorDobleSalto <= 0f;
            puedeSaltar = false;
        }
        else
        {
            // Ya se ha gastado el doble salto de este ciclo.
            puedeUsarDobleSalto = false;
        }

        saltoEnCurso = true;
        manteniendoSalto = true;
        tiempoSalto = 0f;
    }

    private void ActualizarRetardoDobleSalto()
    {
        // La cuenta atrás empieza al realizar el primer salto y termina al permitir el segundo.
        if (!puedeUsarDobleSalto && saltosDisponibles > 0 && !puedeSaltar)
        {
            temporizadorDobleSalto -= Time.deltaTime;

            if (temporizadorDobleSalto <= 0f)
            {
                temporizadorDobleSalto = 0f;
                puedeUsarDobleSalto = true;
            }
        }

        if (panelTemporizadorDobleSalto == null)
            return;

        if (puedeSaltar)
        {
            panelTemporizadorDobleSalto.text = "Doble salto: preparado";
        }
        else if (saltosDisponibles <= 0)
        {
            panelTemporizadorDobleSalto.text = "Doble salto: utilizado";
        }
        else if (!puedeUsarDobleSalto)
        {
            panelTemporizadorDobleSalto.text = "Doble salto en: "
                + temporizadorDobleSalto.ToString("F1") + " s";
        }
        else
        {
            panelTemporizadorDobleSalto.text = "Doble salto: disponible";
        }
    }

    private void ActualizarBarraVida()
    {
        if (barraVida == null)
            return;

        barraVida.maxValue = vidaMaxima;
        barraVida.value = vidaActual;
    }

    private void ActualizarPanelVidas()
    {
        if (panelVidas != null)
            panelVidas.text = "Vidas: " + vidas;
    }

    private bool RecibirDano(float cantidad)
    {
        // Mientras el personaje reaparece o es invulnerable, ignora el daño.
        // Recibir daño NO resta una vida: solamente reduce la salud de la barra.
        if (!puedeMoverse || invulnerable || vidaActual <= 0f || cantidad <= 0f)
            return false;

        vidaActual = Mathf.Max(0f, vidaActual - cantidad);
        ActualizarBarraVida();

        if (vidaActual <= 0f)
        {
            // Las vidas solo disminuyen cuando la barra de salud llega a cero.
            PerderVida();
        }
        else
        {
            // Si aún queda salud, conserva el control y activa la invulnerabilidad.
            invulnerable = true;
            temporizadorInvulnerabilidad = duracionInvulnerabilidad;
            temporizadorParpadeo = 0f;
        }

        return true;
    }

    private void PerderVida()
    {
        // Este método solo se ejecuta cuando la barra de salud llega a cero.
        vidas = Mathf.Max(0, vidas - 1);
        ActualizarPanelVidas();

        // Ahora sí se detiene al personaje mientras espera a reaparecer.
        puedeMoverse = false;
        inputHorizontal = 0f;
        puedeSaltar = false;
        saltoEnCurso = false;
        manteniendoSalto = false;
        rb.linearVelocity = Vector2.zero;

        invulnerable = false;
        temporizadorInvulnerabilidad = 0f;
        temporizadorParpadeo = 0f;
        if (spriteJugador != null)
            spriteJugador.enabled = true;

        if (muerteFX != null)
            muerteFX.Play();

        if (vidas <= 0)
        {
            DerrotaDefinitiva();
        }
        else
        {
            // Todavía quedan vidas: reaparecerá con la salud restaurada.
            CancelInvoke(nameof(VolverAlSpawn));
            Invoke(nameof(VolverAlSpawn), 2f);
        }
    }

    private void ProcesarDanioPinchos()
    {
        // Los pinchos solo reducen la barra de salud.
        // El jugador sigue moviéndose mientras conserve al menos un punto de salud.
        // Si permanece encima, puede volver a recibir daño al acabar la invulnerabilidad.
        if (!puedeMoverse || invulnerable)
            return;

        if (RecibirDano(dañoPinchos) && sonidoPinchos != null)
            sonidoPinchos.Play();

        puedeSaltar = true;
    }

    private void ProcesarDanioPinchosGrave()
    {

        if (!puedeMoverse || invulnerable)
            return;

        if (RecibirDano(dañoPinchosgrave) && sonidoPinchos != null)
            sonidoPinchos.Play();

        puedeSaltar = true;
    }

    private void ActualizarInvulnerabilidad()
    {
        if (!invulnerable)
            return;

        temporizadorInvulnerabilidad -= Time.deltaTime;
        temporizadorParpadeo += Time.deltaTime;

        float intervalo = Mathf.Max(0.01f, intervaloParpadeo);

        if (spriteJugador != null)
        {
            while (temporizadorParpadeo >= intervalo)
            {
                temporizadorParpadeo -= intervalo;
                spriteJugador.enabled = !spriteJugador.enabled;
            }
        }

        if (temporizadorInvulnerabilidad <= 0f)
        {
            invulnerable = false;
            temporizadorInvulnerabilidad = 0f;
            temporizadorParpadeo = 0f;

            if (spriteJugador != null)
                spriteJugador.enabled = true;
        }
    }

    private void DerrotaDefinitiva()
    {
        puedeMoverse = false;
        puedeSaltar = false;
        saltoEnCurso = false;
        manteniendoSalto = false;
        rb.linearVelocity = Vector2.zero;

        if (UI_jugador != null)
            UI_jugador.SetActive(false);
        if (pantallaMuerte != null)
            pantallaMuerte.SetActive(true);
        if (musicaNivel != null)
            musicaNivel.Stop();
        if (musicaDerrota != null)
            musicaDerrota.Play();

        CancelInvoke(nameof(VolverAlSpawn));
    }

    private bool EsPisoton(Collision2D collision)
    {
        // El jugador debe estar cayendo y la parte inferior de su collider
        // debe estar aproximadamente a la altura de la parte superior del enemigo.
        if (rb.linearVelocity.y > 0.1f)
            return false;

        if (colliderJugador == null || collision.collider == null)
            return transform.position.y > collision.transform.position.y;

        float parteInferiorJugador = colliderJugador.bounds.min.y;
        float parteSuperiorEnemigo = collision.collider.bounds.max.y;

        return parteInferiorJugador >= parteSuperiorEnemigo - margenPisoton;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bola"))
        {
            Destroy(collision.gameObject);
            puntuacion++;

            if (panelPuntuación != null)
                panelPuntuación.text = "Puntuación: " + puntuacion.ToString();

            Debug.Log("Puntuación: " + puntuacion + " puntos");
        }

        // Los pinchos únicamente restan salud. No se descuenta una vida aquí
        // y tampoco se bloquea el movimiento mientras quede salud.
        if (collision.gameObject.CompareTag("Pinchos"))
        {
            Debug.Log("El personaje ha tocado los pinchos");
            ProcesarDanioPinchos();
        }

        // Los pinchos graves únicamente restan salud. No se descuenta una vida aquí
        // y tampoco se bloquea el movimiento mientras quede salud.
        if (collision.gameObject.CompareTag("PinchosGrave"))
        {
            Debug.Log("El personaje ha tocado los pinchos graves");
            ProcesarDanioPinchosGrave();
            return;
        }

        // Enemigos: pisarlos desde arriba los destruye; tocarlos lateralmente resta vida.
        if (collision.gameObject.CompareTag("Enemigo") && puedeMoverse)
        {
            if (EsPisoton(collision))
            {
                Destroy(collision.gameObject);
                sonidoEnemigo.Play();
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaReboteEnemigo);
                saltoEnCurso = false;
                manteniendoSalto = false;

            }
            else
            {
                RecibirDano(dañoEnemigo);
            }

            return;
        }

        // Restablecer los saltos únicamente al aterrizar sobre el suelo.
        if (collision.gameObject.CompareTag("Suelo") && puedeMoverse)
        {
            bool haAterrizado = false;

            foreach (ContactPoint2D contacto in collision.contacts)
            {
                if (contacto.normal.y > 0.5f)
                {
                    haAterrizado = true;
                    break;
                }
            }

            if (haAterrizado)
            {
                Debug.Log("Toca el suelo");
                puedeSaltar = true;
                saltosDisponibles = saltosMaximos;
                puedeUsarDobleSalto = true;
                temporizadorDobleSalto = 0f;
            }
        }

        if (collision.gameObject.CompareTag("Final"))
        {
            puedeMoverse = false;
            puedeSaltar = false;
            saltoEnCurso = false;
            pantallaFin.SetActive(true);
            //Espera un tiempo antes de cargar la escena del siguiente nivel
            Invoke("CargarSiguienteNivel", 2f);
        }
    }

    private void CargarSiguienteNivel()
    {
        SceneManager.LoadScene("Nivel2");
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Si el jugador permanece sobre los pinchos, vuelve a recibir daño
        // cuando termine el periodo de invulnerabilidad.
        if (collision.gameObject.CompareTag("Pinchos"))
            ProcesarDanioPinchos();
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            Debug.Log("No toca");
            puedeSaltar = false;
        }
    }

    private void pausar()
    {
        Time.timeScale = 0f;
        pausa.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ParedInvisible"))
        {
            Debug.Log("Detecta una pared invisible");
        }
        else if (collision.gameObject.CompareTag("Moneda"))
        {
            Destroy(collision.gameObject);
            monedas++;

            if (panelPuntuación != null)
                panelPuntuación.text = "Monedas: " + monedas.ToString();

            Debug.Log("Monedas: " + monedas);

            if (dineroFX != null)
                dineroFX.Play();
            if (sonidoMoneda != null)
                sonidoMoneda.Play();
        }

        if (collision.gameObject.CompareTag("Trigger"))
        {
            Debug.Log("Toca el trigger");
            advertencia.SetActive(true);
        }

        if (collision.gameObject.CompareTag("Key"))
        {
            Destroy(collision.gameObject);
            haskey = true;

            if (sonidoObtener != null)
                sonidoObtener.Play();
            if (puerta != null)
                puerta.SetActive(false);
            if (textopuerta != null)
                textopuerta.SetActive(false);

            Debug.Log("Has obtenido la llave");

            if (keyUI != null)
                keyUI.SetActive(true);
        }

        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            if (CheckPoint != null &&
                (collision.transform == CheckPoint.transform ||
                 collision.transform.IsChildOf(CheckPoint.transform)))
            {
                spawnActual = spawnPoint;
                Debug.Log("Checkpoint 1 activado");
            }
            else if (CheckPoint2 != null &&
                     (collision.transform == CheckPoint2.transform ||
                      collision.transform.IsChildOf(CheckPoint2.transform)))
            {
                spawnActual = SpawnPoint2;
                Debug.Log("Checkpoint 2 activado");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Moneda"))
            Debug.Log("Detecta que estoy en el trigger");

        if (collision.gameObject.CompareTag("ParedInvisible"))
            Debug.Log("Detecta una pared invisible");

        if (collision.gameObject.CompareTag("Trigger"))
        {
            Debug.Log("Sale del trigger");
            advertencia.SetActive(false);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Moneda"))
            Debug.Log("Detecta que estoy en el trigger");

        if (collision.gameObject.CompareTag("ParedInvisible"))
            Debug.Log("Detecta una pared invisible");
    }

    private void VolverAlSpawn()
    {
        if (spawnActual == null)
            spawnActual = spawnPoint;

        if (spawnActual != null)
        {
            rb.position = spawnActual.position;
            Debug.Log("Reapareciendo en: " + spawnActual.name);
        }
        else
        {
            Debug.LogError("No hay ningún SpawnPoint asignado");
        }

        rb.linearVelocity = Vector2.zero;

        vidaActual = vidaMaxima;
        ActualizarBarraVida();

        invulnerable = false;
        temporizadorInvulnerabilidad = 0f;
        temporizadorParpadeo = 0f;

        if (spriteJugador != null)
            spriteJugador.enabled = true;

        saltoEnCurso = false;
        manteniendoSalto = false;
        tiempoSalto = 0f;

        puedeSaltar = false;
        saltosDisponibles = saltosMaximos;
        puedeUsarDobleSalto = true;
        temporizadorDobleSalto = 0f;

        puedeMoverse = true;
    }
}
