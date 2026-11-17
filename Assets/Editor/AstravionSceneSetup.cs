
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AstravionSceneSetup
{
    private const string ScenePath = "Assets/Scenes/Astravion.unity";

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.delayCall += CreateScene;
    }

    [MenuItem("ASTRAVION/Create Game Scene")]
    public static void CreateScene()
    {
        if (EditorApplication.isCompiling)
            return;

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        Scene scene = EditorSceneManager.NewScene(
            NewSceneSetup.DefaultGameObjects,
            NewSceneMode.Single
        );

        GameObject root = new GameObject("ASTRAVION_BOOTSTRAP");

        Type bootstrap = Type.GetType(
            "AstravionBootstrap, Assembly-CSharp"
        );

        if (bootstrap != null &&
            typeof(MonoBehaviour).IsAssignableFrom(bootstrap))
        {
            root.AddComponent(bootstrap);
        }
        else
        {
            Debug.LogError("AstravionBootstrap belum ditemukan.");
        }

        EditorSceneManager.SaveScene(scene, ScenePath);

        AssetDatabase.Refresh();

        string sceneGuid = AssetDatabase.AssetPathToGUID(ScenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        AssetDatabase.SaveAssets();

        Debug.Log("ASTRAVION Scene dibuat. GUID: " + sceneGuid);
    }
}

public class AstravionBuildScenePreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (!File.Exists(ScenePathForBuild))
            throw new BuildFailedException(
                "ASTRAVION Scene belum dibuat. Jalankan menu ASTRAVION/Create Game Scene."
            );

        if (EditorBuildSettings.scenes.Length == 0)
            throw new BuildFailedException(
                "Build Settings belum memiliki Scene."
            );
    }

    private const string ScenePathForBuild =
        "Assets/Scenes/Astravion.unity";
}
