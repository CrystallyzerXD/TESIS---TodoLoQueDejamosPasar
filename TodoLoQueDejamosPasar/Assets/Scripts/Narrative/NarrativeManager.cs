using UnityEngine;
using System.Collections.Generic;

// Registra que interacciones fueron completadas.
// Vive en Bootstrap junto con GameManager.
// Los triggers lo consultan para verificar requisitos.

public class NarrativeManager : MonoBehaviour
{
    public static NarrativeManager Instance { get; private set; }

    private readonly HashSet<string> completadas = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void MarcarCompletada(string id)
    {
        if (!string.IsNullOrEmpty(id))
            completadas.Add(id);
    }

    public bool EstaCompletada(string id)
    {
        if (string.IsNullOrEmpty(id)) return true;
        return completadas.Contains(id);
    }

    public bool TodasCompletadas(string[] ids)
    {
        if (ids == null || ids.Length == 0) return true;
        foreach (var id in ids)
            if (!EstaCompletada(id)) return false;
        return true;
    }

    /// <summary>
    /// Limpia todas las interacciones completadas.
    /// Llamar al iniciar una nueva partida.
    /// </summary>
    public void Resetear()
    {
        completadas.Clear();
    }
}