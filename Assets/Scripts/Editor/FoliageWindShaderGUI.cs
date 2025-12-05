using UnityEditor;
using UnityEngine;

public class FoliageWindShaderGUI : ShaderGUI
{
    private MaterialEditor m_materialEditor;
    private MaterialProperty[] m_properties;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        m_materialEditor = materialEditor;
        m_properties = properties;

        Material targetMat = materialEditor.target as Material;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Foliage Wind Shader", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        DrawProperty("_MainTex", "Albedo Texture");
        DrawProperty("_BaseColor", "Base Color");
        DrawProperty("_SecondColor", "Secondary Color");
        DrawProperty("_ColorVariation", "Color Variation");
        DrawProperty("_AlphaClip", "Alpha Clip");

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Wind Settings", EditorStyles.boldLabel);

        DrawProperty("_WindStrength", "Wind Strength");
        DrawProperty("_WindSpeed", "Wind Speed");
        DrawProperty("_WindFrequency", "Wind Frequency");
        DrawProperty("_WindDirection", "Wind Direction");
        DrawProperty("_WindNoiseScale", "Wind Noise Scale");
        DrawProperty("_WindZoneInfluence", "Wind Zone Influence");

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Movement Settings", EditorStyles.boldLabel);

        DrawProperty("_BaseSway", "Base Sway");
        DrawProperty("_LeafFlutter", "Leaf Flutter");
        DrawProperty("_StemStiffness", "Stem Stiffness");

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);

        DrawProperty("_ReceiveShadows", "Receive Shadows");
        
        MaterialProperty cullMode = FindProperty("_Cull", properties);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Cull Mode");
        cullMode.floatValue = EditorGUILayout.Popup((int)cullMode.floatValue,
            new string[] { "Off", "Front", "Back" });
        EditorGUILayout.EndHorizontal();

        // Double Sided
        MaterialProperty doubleSided = FindProperty("_DoubleSided", properties);
        if (doubleSided != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Double Sided");
            doubleSided.floatValue = EditorGUILayout.Toggle(doubleSided.floatValue > 0.5f) ? 1.0f : 0.0f;
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Vertex Color Usage:\n" +
            "• Red: Stiffness (0=flexible leaves, 1=stiff stems)\n" +
            "• Green: Color gradient variation\n" +
            "• Blue: (Optional) Additional wind variation",
            MessageType.Info);
        
        if (GUILayout.Button("Optimize for Performance"))
        {
            targetMat.enableInstancing = true;
            EditorUtility.SetDirty(targetMat);
            Debug.Log("Material optimized for instancing");
        }
    }

    private void DrawProperty(string propertyName, string label)
    {
        MaterialProperty prop = FindProperty(propertyName, m_properties);
        if (prop != null)
        {
            m_materialEditor.ShaderProperty(prop, label);
        }
    }
}