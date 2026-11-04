
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonTextColor : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    public Color colorNormal = Color.white;
    public Color colorHover = Color.yellow;
    public Color colorClick = Color.red;

    [Header("Sonido")]
    public AudioSource sonidoClick;
    [Range(0f, 1f)]
    public float volumen = 1f;

    private Graphic texto;
    private AudioSource audioSource;

    private bool ratonEncima;
    private bool pulsado;

    void Awake()
    {
        Transform hijoTexto = transform.Find("Text");

        if (hijoTexto == null)
            hijoTexto = transform.Find("Text (TMP)");

        if (hijoTexto != null)
            texto = hijoTexto.GetComponent<Graphic>();

        if (texto == null)
            texto = GetComponentInChildren<TMP_Text>();

        if (texto == null)
            texto = GetComponentInChildren<Text>();

        ActualizarColor();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ratonEncima = true;
        ActualizarColor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ratonEncima = false;
        ActualizarColor();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pulsado = true;
        sonidoClick.Play();
        ActualizarColor();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pulsado = false;
        ActualizarColor();
    }

    private void ActualizarColor()
    {
        if (texto == null) return;

        texto.color = pulsado ? colorClick :
                      ratonEncima ? colorHover :
                      colorNormal;
    }

}
