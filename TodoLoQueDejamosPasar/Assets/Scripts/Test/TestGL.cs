using UnityEngine;

[ExecuteAlways]
public class TestGL : MonoBehaviour
{
    private Material mat;

    private void Awake()
    {
        mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
    }

    private void OnRenderObject()
    {
        Debug.Log("TestGL OnRenderObject");
        mat.SetPass(0);
        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        GL.Vertex3(0f, 0f, 0f);
        GL.Vertex3(1f, 1f, 0f);
        GL.End();
        GL.PopMatrix();
    }
}