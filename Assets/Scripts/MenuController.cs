using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject botonReanudar;
    [SerializeField] GameObject botonReintentar;
    [SerializeField] GameObject botonOpciones;
    [SerializeField] GameObject menuPrincipal;
    [SerializeField] GameObject confirmarSalir;
    [SerializeField] GameObject VolverMenu;
    [SerializeField] GameObject Pausa;
    [SerializeField] GameObject Stats;
    [SerializeField] GameObject Confirmar;
    [SerializeField] GameObject Opciones;

    public void reanudar()
    {
        //Cargar escena actual
        Pausa.SetActive(false);
        Stats.SetActive(true);
        Time.timeScale = 1f;
        Debug.Log("Pausa reanudada");
    }
    public void reintentar()
    {
        //Cargar escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }

    public void entrarOpciones()
    {
        Opciones.SetActive(true);
        Pausa.SetActive(false);
    }


    public void salirOpciones()
    {
        Opciones.SetActive(false);
        Pausa.SetActive(true);
    }

    public void Salir()
    {
        Application.Quit();
    }

    private void SalirPartida()
    {
        Pausa.SetActive(false);
        Confirmar.SetActive(true);
    }

    private void AcabarPartida()
    {
        SceneManager.LoadScene("Menu principal");
    }

    public void mostrarPausa()
    {
        //Cargar escena de instrucciones
        Pausa.SetActive(true);
        menuPrincipal.SetActive(false);
    }

    public void SalirMenu()
    {
        //Cargar escena de menu principal
        Pausa.SetActive(false);
        Stats.SetActive(true);
        menuPrincipal.SetActive(true);
    }



}
