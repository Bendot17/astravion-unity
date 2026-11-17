
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AstravionProjectSetup
{
    static AstravionProjectSetup()
    {
        EditorApplication.delayCall += EnsureProjectReady;
    }

    static void EnsureProjectReady()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        const string sceneFolder = "Assets/Scenes";
        const string scenePath = "Assets/Scenes/AstravionArena.unity";
        if (!Directory.Exists(sceneFolder)) Directory.CreateDirectory(sceneFolder);

        Scene scene;
        if (!File.Exists(scenePath))
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("ASTRAVION GAME BOOTSTRAP");
            root.AddComponent<AstravionBootstrap>();
            EditorSceneManager.SaveScene(scene, scenePath);
        }
        else
        {
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (Object.FindObjectOfType<AstravionBootstrap>() == null)
            {
                new GameObject("ASTRAVION GAME BOOTSTRAP").AddComponent<AstravionBootstrap>();
                EditorSceneManager.SaveScene(scene, scenePath);
            }
        }

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(scenePath, true) };
        PlayerSettings.productName = "ASTRAVION";
        PlayerSettings.companyName = "Developed by Suhendra";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "id.astravion.moba");
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        AssetDatabase.SaveAssets();
    }
}
