#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NPCSchedule))]
public class NPCScheduleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        NPCSchedule schedule = (NPCSchedule)target;

        // Ubicacion
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("locationID"),
            new GUIContent("Escena")
        );

        EditorGUILayout.Space(4);

        // Casilla esExtra con color
        Color colorAnterior = GUI.backgroundColor;
        GUI.backgroundColor = schedule.esExtra
            ? new Color(0.6f, 0.9f, 1f)
            : new Color(0.8f, 1f, 0.7f);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("esExtra"),
            new GUIContent(
                schedule.esExtra
                    ? "✦ NPC Extra (por bienestar)"
                    : "★ Flujo Principal (por dias)",
                schedule.esExtra
                    ? "Aparece cuando el bienestar cumple el rango. No depende del dia."
                    : "Aparece segun los dias configurados, cada uno con su condicion de hora."
            )
        );
        EditorGUILayout.EndVertical();

        GUI.backgroundColor = colorAnterior;

        EditorGUILayout.Space(8);

        if (schedule.esExtra)
        {
            EditorGUILayout.LabelField("Condicion de bienestar", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("bienestarMin"),
                new GUIContent("Bienestar Minimo", "-1 = sin limite")
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("bienestarMax"),
                new GUIContent("Bienestar Maximo", "-1 = sin limite")
            );

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Hora del dia", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("hora"),
                new GUIContent("Aparece")
            );
        }
        else
        {
            EditorGUILayout.LabelField("Dias en que aparece", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Cada elemento tiene su propio dia y condicion de hora.",
                MessageType.None
            );
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("diasActivos"),
                new GUIContent("Dias Activos"),
                true
            );
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif