using UnityEngine;
using UnityEditor;

public class MaterialShaderSwitcher : EditorWindow
{
    private Material selectedMaterial;
    private string targetShaderName = "Particles/Standard Unlit";

    [MenuItem("Tools/Material Shader Switcher")]
    public static void ShowWindow()
    {
        // Show the window
        GetWindow<MaterialShaderSwitcher>("Material Shader Switcher");
    }

    private void OnGUI()
    {
        GUILayout.Label("Shader Switcher", EditorStyles.boldLabel);

        // Automatically update the selected material in the project
        UpdateSelectedMaterial();

        // Display the currently selected material
        EditorGUILayout.ObjectField("Selected Material", selectedMaterial, typeof(Material), false);

        // Enter the target shader name
        targetShaderName = EditorGUILayout.TextField("Target Shader", targetShaderName);

        // Button to switch shader
        if (GUILayout.Button("Switch Shader") && selectedMaterial != null)
        {
            SwitchShader();
        }
    }

    private void UpdateSelectedMaterial()
    {
        // Check if the selected object is a material
        if (Selection.activeObject is Material)
        {
            selectedMaterial = (Material)Selection.activeObject;
        }
        else
        {
            selectedMaterial = null; // Clear if the selection is not a material
        }
    }

    private void SwitchShader()
    {
        // Find the shader by name
        Shader targetShader = Shader.Find(targetShaderName);

        if (targetShader != null)
        {
            // Switch the material's shader to the target shader
            selectedMaterial.shader = targetShader;
            Debug.Log($"Shader switched to: {targetShaderName}");
        }
        else
        {
            Debug.LogWarning($"Shader not found: {targetShaderName}");
        }
    }

    private void OnSelectionChange()
    {
        // Force the window to repaint when the selection changes
        Repaint();
    }
}
