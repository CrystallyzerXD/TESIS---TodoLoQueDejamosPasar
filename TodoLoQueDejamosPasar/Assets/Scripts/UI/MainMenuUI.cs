using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Tooltip("Escena de rol que se muestra como intro al iniciar partida nueva")]
    public EscenaRolData introData;

    private void Start()
    {
        AudioManager.Instance.ReproducirMenuPrincipal();
    }

    public void StartGame()
    {
        GameManager.Instance.escenaActual    = Escenas.Home;
        GameManager.Instance.llegoPorDerecha = false;

        if (introData != null)
        {
            GameManager.Instance.escenaRolPendiente = introData;
            GameManager.Instance.vieneDeReflexion   = true;
            GameManager.Instance.introPendiente      = true;
        }

        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}