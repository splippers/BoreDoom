using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;

/// <summary>
/// Creates XR Plug-in Management assets when they are missing from the repo (broken GUID refs).
/// Fixes: "No XR Manager settings found, manifest entries will not be updated."
/// Runs once on Editor startup (delayCall).
/// </summary>
[InitializeOnLoad]
public static class BoreDoomXrManagementBootstrap
{
    const string XrRoot = "Assets/XR";
    const string SettingsFolder = XrRoot + "/Settings";
    const string LoadersFolder = XrRoot + "/Loaders";
    const string PerBuildAssetPath = SettingsFolder + "/XRGeneralSettingsPerBuildTarget.asset";
    const string OpenXrLoaderPath = LoadersFolder + "/Open XR Loader.asset";

    static BoreDoomXrManagementBootstrap()
    {
        EditorApplication.delayCall += EnsureXrManagementAssetsExist;
    }

    static void EnsureXrManagementAssetsExist()
    {
        try
        {
            EnsureFolders();

            var loaderPath = EnsureOpenXrLoaderAsset();
            var loader = AssetDatabase.LoadAssetAtPath<XRLoader>(loaderPath);
            if (loader == null)
            {
                Debug.LogError("[BoreDoom] Failed to load OpenXR Loader at " + loaderPath);
                return;
            }

            var perBuild = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(PerBuildAssetPath);
            if (perBuild == null)
            {
                perBuild = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                AssetDatabase.CreateAsset(perBuild, PerBuildAssetPath);
            }

            foreach (var group in new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android })
            {
                if (!perBuild.HasSettingsForBuildTarget(group))
                    perBuild.CreateDefaultSettingsForBuildTarget(group);
                if (!perBuild.HasManagerSettingsForBuildTarget(group))
                    perBuild.CreateDefaultManagerSettingsForBuildTarget(group);
            }

            bool androidReady = IsAndroidLoaderConfigured(perBuild);
            if (androidReady)
            {
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perBuild, true);
                TryEnsureOpenXrPackageSettings();
                return;
            }

            foreach (var group in new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android })
            {
                var mgr = perBuild.ManagerSettingsForBuildTarget(group);
                if (mgr == null)
                    continue;
                while (mgr.loaders.Count > 0)
                    mgr.TryRemoveLoader(mgr.loaders[0]);
                mgr.TryAddLoader(loader);
                mgr.automaticLoading = true;
                mgr.automaticRunning = true;
            }

            foreach (var group in new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android })
            {
                var gs = perBuild.SettingsForBuildTarget(group);
                if (gs != null)
                    gs.InitManagerOnStart = true;
            }

            EditorUtility.SetDirty(perBuild);
            AssetDatabase.SaveAssets();

            EditorBuildSettings.RemoveConfigObject(XRGeneralSettings.k_SettingsKey);
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perBuild, true);

            TryEnsureOpenXrPackageSettings();
            AssetDatabase.Refresh();
            Debug.Log("[BoreDoom] XR Plug-in Management: OpenXR loader registered for Standalone + Android. Rebuild the Android player to refresh the manifest.");
        }
        catch (Exception e)
        {
            Debug.LogError("[BoreDoom] XR bootstrap failed: " + e);
        }
    }

    static bool IsAndroidLoaderConfigured(XRGeneralSettingsPerBuildTarget perBuild)
    {
        var mgr = perBuild.ManagerSettingsForBuildTarget(BuildTargetGroup.Android);
        return mgr != null && mgr.loaders != null && mgr.loaders.Count > 0;
    }

    static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(XrRoot))
            AssetDatabase.CreateFolder("Assets", "XR");
        if (!AssetDatabase.IsValidFolder(LoadersFolder))
            AssetDatabase.CreateFolder(XrRoot, "Loaders");
        if (!AssetDatabase.IsValidFolder(SettingsFolder))
            AssetDatabase.CreateFolder(XrRoot, "Settings");
    }

    static string EnsureOpenXrLoaderAsset()
    {
        var full = Path.Combine(Directory.GetCurrentDirectory(), OpenXrLoaderPath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(full))
            return OpenXrLoaderPath;

        var instance = ScriptableObject.CreateInstance<OpenXRLoader>();
        AssetDatabase.CreateAsset(instance, OpenXrLoaderPath);
        AssetDatabase.SaveAssets();
        return OpenXrLoaderPath;
    }

    /// <summary>OpenXRPackageSettings is internal; invoke GetOrCreateInstance so OpenXR package settings exist.</summary>
    static void TryEnsureOpenXrPackageSettings()
    {
        const string editorAsm = "Unity.XR.OpenXR.Editor";
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name != editorAsm)
                continue;
            var t = assembly.GetType("UnityEditor.XR.OpenXR.OpenXRPackageSettings");
            if (t == null)
                return;
            var m = t.GetMethod("GetOrCreateInstance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            m?.Invoke(null, null);
            return;
        }
    }
}
