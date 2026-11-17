
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AstravionSceneSetup
{
    private const string ScenePath = "Assets/Scenes/Astravion.unity";

    static AstravionSceneSetup()
    {
        EditorApplication.delayCall += EnsureScene;
    }

    public static void EnsureScene()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        Scene scene;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);
        else
            scene = EditorSceneManager.OpenScene(
                ScenePath, OpenSceneMode.Single);

        GameObject root = GameObject.Find("ASTRAVION_BOOTSTRAP");

        if (root == null)
            root = new GameObject("ASTRAVION_BOOTSTRAP");

        Type bootstrapType = FindBootstrapType();

        if (bootstrapType != null &&
            typeof(MonoBehaviour).IsAssignableFrom(bootstrapType))
        {
            if (root.GetComponent(bootstrapType) == null)
                root.AddComponent(bootstrapType);
        }
        else
        {
            Debug.LogError("AstravionBootstrap tidak ditemukan.");
        }

        EditorSceneManager.SaveScene(scene, ScenePath);

        var scenes = EditorBuildSettings.scenes.ToList();

        if (!scenes.Any(s => s.path == ScenePath && s.enabled))
        {
            scenes.RemoveAll(s => s.path == ScenePath);
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        AssetDatabase.SaveAssets();
        Debug.Log("ASTRAVION Scene berhasil disiapkan.");
    }

    private static Type FindBootstrapType()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType("AstravionBootstrap");
            if (type != null)
                return type;
        }

        return null;
    }
}

public class AstravionBuildScenePreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        AstravionSceneSetup.EnsureScene();
    }
}
