using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// Script temporal de diagnostico — borrar despues de resolver el problema.
/// Ponlo en cualquier GameObject de GameScene y mira la consola al pausar.
public class DebugClick : MonoBehaviour
{
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Debug.Log($"[DebugClick] Click en posicion: {Input.mousePosition}");

        // Raycast contra todos los elementos UI
        var es = EventSystem.current;
        if (es == null) { Debug.LogError("[DebugClick] NO HAY EventSystem.current"); return; }

        Debug.Log($"[DebugClick] EventSystem activo: {es.gameObject.name} | enabled: {es.enabled}");

        var resultados = new List<RaycastResult>();
        var datos      = new PointerEventData(es) { position = Input.mousePosition };
        es.RaycastAll(datos, resultados);

        if (resultados.Count == 0)
        {
            Debug.LogWarning("[DebugClick] Raycast no golpeo nada — el GraphicRaycaster no ve los botones");
        }
        else
        {
            foreach (var r in resultados)
                Debug.Log($"[DebugClick] Golpeo: {r.gameObject.name} en canvas: {r.module?.gameObject.name}");
        }
    }
}