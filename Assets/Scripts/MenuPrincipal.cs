using System.Threading;
using Unity.Collections.Tests.CoreCLR.TestJobs;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] GameObject botonNuevaPartida;
    [SerializeField] GameObject botonCargarPartida;
    [SerializeField] GameObject botonOpciones;
    [SerializeField] GameObject botonSalir;
    [SerializeField] GameObject PanelMenu;
    [SerializeField] GameObject PanelOpciones;
    [SerializeField] GameObject PanelSalir;
    [SerializeField] GameObject ConfirmarSalir;
    [SerializeField] GameObject CancelarSalir;
    [SerializeField] GameObject Menu_Principal;
    [SerializeField] GameObject Cargando;
    [SerializeField] GameObject SelectorNivel;

    public void NuevaPartida()
    {
        PanelMenu.SetActive(false);
        Cargando.SetActive(true);
        Thread.Sleep(5000);
        //Cargar escena actual
        SceneManager.LoadScene("Nivel1");
    }

    public void Nivel2()
    {
        PanelMenu.SetActive(false);
        Cargando.SetActive(true);
        Thread.Sleep(5000);
        //Cargar escena actual
        SceneManager.LoadScene("Nivel2");
    }


    public void entrarOpciones()
    {
        PanelOpciones.SetActive(true);
        Menu_Principal.SetActive(false);
    }
    public void salirOpciones()
    {
        PanelOpciones.SetActive(false);
        Menu_Principal.SetActive(true);
    }

    public void Salir()
    {
        Menu_Principal.SetActive(false);
        ConfirmarSalir.SetActive(true);
    }

    private void SalirJuego()
    {
        Application.Quit();
    }

    private void cancelarSalirJuego()
    {
        Menu_Principal.SetActive(true);
        ConfirmarSalir.SetActive(false);
    }

    public void abrirSelector()
    {
        SelectorNivel.SetActive(true);
        PanelMenu.SetActive(false);
    }
    public void SalirSelector()
    {
        SelectorNivel.SetActive(false);
        PanelMenu.SetActive(true);
    }
}
