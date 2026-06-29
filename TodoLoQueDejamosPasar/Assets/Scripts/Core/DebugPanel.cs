using UnityEngine;

public class DebugPanel : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Referencias")]
    public SceneController sceneController;

    private void Update()
    {
        bool cambio      = false;
        bool cambioNoche = false;

        if (Input.GetKeyDown(KeyCode.Q)) { GameManager.Instance.ModificarContaminacion(+1);    cambio = true; }
        if (Input.GetKeyDown(KeyCode.A)) { GameManager.Instance.ModificarContaminacion(-1);    cambio = true; }

        if (Input.GetKeyDown(KeyCode.W)) { GameManager.Instance.ModificarBienestar(+1);        cambio = true; }
        if (Input.GetKeyDown(KeyCode.S)) { GameManager.Instance.ModificarBienestar(-1);        cambio = true; }

        if (Input.GetKeyDown(KeyCode.R)) { GameManager.Instance.ModificarRiesgoInundacion(+1); cambio = true; }
        if (Input.GetKeyDown(KeyCode.F)) { GameManager.Instance.ModificarRiesgoInundacion(-1); cambio = true; }

        if (Input.GetKeyDown(KeyCode.Alpha1)) { GameManager.Instance.ModificarEstadoArboles(1); cambio = true; }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { GameManager.Instance.ModificarEstadoArboles(2); cambio = true; }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { GameManager.Instance.ModificarEstadoArboles(3); cambio = true; }

        if (Input.GetKeyDown(KeyCode.N))
        {
            GameManager.Instance.esNoche = !GameManager.Instance.esNoche;
            cambioNoche = true;
        }

        // 0 → avanza un dia (1→2→3→4→5→6→7→1)
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            GameManager.Instance.diaActual = (GameManager.Instance.diaActual % 7) + 1;
            cambioNoche = true;
        }

        // L → toggle lluvia
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (RainManager.Instance != null)
            {
                if (RainManager.Instance.EstaActiva())
                    RainManager.Instance.DesactivarLluvia();
                else
                    RainManager.Instance.ActivarLluvia();
            }
            else
            {
                Debug.LogWarning("DebugPanel: RainManager.Instance es null. ¿Está el objeto en la escena?");
            }
        }

        // P → testea FadeOutIn
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (FadeManager.Instance != null)
                FadeManager.Instance.FadeOutIn(1f, 0.5f, null, null);
        }

        if (sceneController != null)
        {
            if (cambioNoche)
                sceneController.RefrescarTodo();
            else if (cambio)
                sceneController.RefrescarVisual();
        }
    }

    private void OnGUI()
    {
        if (GameManager.Instance == null) return;

        string estadoLluvia = RainManager.Instance != null
            ? (RainManager.Instance.EstaActiva() ? "ON" : "OFF")
            : "sin RainManager";

        GUILayout.BeginArea(new Rect(10, 10, 300, 280));
        GUILayout.Box(
            $"[DEBUG PANEL]\n" +
            $"Escena:      {GameManager.Instance.escenaActual}\n" +
            $"Dia:         {GameManager.Instance.diaActual}           (0)\n" +
            $"Noche:       {GameManager.Instance.esNoche}        (N)\n" +
            $"Cont:        {GameManager.Instance.contaminacion}           (Q / A)\n" +
            $"Bienestar:   {GameManager.Instance.bienestarUrbano}           (W / S)\n" +
            $"Riesgo:      {GameManager.Instance.riesgoInundacion}           (R / F)\n" +
            $"Arboles:     {GameManager.Instance.estadoArboles}           (1 / 2 / 3)\n" +
            $"Lluvia:      {estadoLluvia}        (L)\n" +
            $"Fade:        (P)"
        );
        GUILayout.EndArea();
    }
#endif
}