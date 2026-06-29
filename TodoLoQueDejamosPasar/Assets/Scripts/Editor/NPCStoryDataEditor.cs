// Este archivo debe estar en Assets/Editor/
// Unity lo detecta automaticamente como editor personalizado.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NPCStoryData))]
public class NPCStoryDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        NPCStoryData data = (NPCStoryData)target;

        // Nombre del NPC
        EditorGUILayout.PropertyField(serializedObject.FindProperty("nombreNPC"));

        EditorGUILayout.Space(4);

        // Casilla esExtra con color de fondo segun estado
        Color colorAnterior = GUI.backgroundColor;
        GUI.backgroundColor = data.esExtra
            ? new Color(0.6f, 0.9f, 1f)   // azul claro = NPC extra
            : new Color(0.8f, 1f, 0.7f);   // verde claro = flujo principal

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("esExtra"),
            new GUIContent(
                data.esExtra
                    ? "✦ NPC Extra (ambiental)"
                    : "★ Flujo Principal",
                data.esExtra
                    ? "Interacciones por variables. Solo dialogo, sin decisiones."
                    : "Interacciones por dia. Puede tener decisiones y bloquear triggers."
            )
        );
        EditorGUILayout.EndVertical();

        GUI.backgroundColor = colorAnterior;

        EditorGUILayout.Space(8);

        // Muestra solo el array correspondiente
        if (data.esExtra)
        {
            EditorGUILayout.LabelField("Interacciones por variables", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Se evaluan de arriba a abajo. Ordenalas de condicion mas restrictiva a menos restrictiva.",
                MessageType.Info
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("interaccionesExtra"),
                new GUIContent("Interacciones"),
                true
            );
        }
        else
        {
            EditorGUILayout.LabelField("Interacciones por dia", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Se evaluan de arriba a abajo. Ordenalas cronologicamente por dia.",
                MessageType.Info
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("interaccionesPrincipales"),
                new GUIContent("Interacciones"),
                true
            );
        }

        EditorGUILayout.Space(4);

        // Texto sin interaccion (siempre visible)
        EditorGUILayout.PropertyField(serializedObject.FindProperty("textoSinInteraccion"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif