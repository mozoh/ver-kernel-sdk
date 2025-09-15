using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.anotherworld.kernel {
    public static class AssetBundlesUtils {
        [MenuItem("Kernel/AssetBundles/Build")]
        private static void BuildAddressablesForAllPlatforms() {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null) {
                Debug.LogError("AddressableAssetSettings not found. Make sure Addressables are set up in your project.");
            return;
            }

            AddressableAssetSettings.CleanPlayerContent();
            AddressableAssetSettings.BuildPlayerContent();

            var sourcePath = $"Library/com.unity.addressables/aa/{(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ? "Android" : "Windows")}/catalog.json";
            var destinationPath = $"Build/AssetBundles/{EditorUserBuildSettings.activeBuildTarget}/catalog.json";
            File.Copy(sourcePath, destinationPath, true);
        }

        [MenuItem("Kernel/AssetBundles/Deploy Windows")]
        private static void DeployWindows() {
            const string sourceDirectory = "Build/AssetBundles/StandaloneWindows64";

            var destinationBaseFolder = EditorUtility.OpenFolderPanel(
                "Select Deployment Directory",
                Application.dataPath,
                ""
            );

            if (string.IsNullOrEmpty(destinationBaseFolder)) {
                Debug.LogWarning("Deployment canceled: no folder selected.");
                return;
            }

            var destinationDirectory = Path.Combine(destinationBaseFolder, "AssetBundles/StandaloneWindows64/Custom");

            try {
                if (!Directory.Exists(sourceDirectory)) {
                    Debug.LogError($"Source directory not found: {sourceDirectory}");
                    return;
                }

                if (!Directory.Exists(destinationDirectory)) Directory.CreateDirectory(destinationDirectory);


                CopyAll(new DirectoryInfo(sourceDirectory), new DirectoryInfo(destinationDirectory));
                Debug.Log($"Windows AssetBundles successfully deployed to: {destinationDirectory}");
            } catch (Exception ex) {
                Debug.LogError($"An error occurred during deployment: {ex.Message}");
            }
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target) {
            foreach (var file in source.GetFiles()) file.CopyTo(Path.Combine(target.FullName, file.Name), true);


            foreach (var subDirectory in source.GetDirectories()) {
                var nextTargetSubDir = target.CreateSubdirectory(subDirectory.Name);
                CopyAll(subDirectory, nextTargetSubDir);
            }
        }

        [MenuItem("Kernel/AssetBundles/Deploy Android")]
        private static void DeployAndroidAssetBundles() {
            var sourceDirectory = Path.Combine(Application.dataPath, "../Build/AssetBundles/Android");
            var appId = EditorInputDialog.Show("AppId", "Please enter appId", "com.anotherworld.kernel");

            var destinationPath = "/sdcard/Android/data/" + appId + "/files/";

            try {
                if (!Directory.Exists(sourceDirectory)) {
                    Debug.LogError($"Source directory not found: {sourceDirectory}");
                    return;
                }

                var files = Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories);

                if (files.Length == 0) {
                    Debug.LogWarning("No files found in the source directory to deploy.");
                    return;
                }

                foreach (var file in files) {
                    var relativePath = file.Substring(sourceDirectory.Length + 1).Replace("\\", "/");
                    var fullDestPath = destinationPath + relativePath;
                    var destDir = Path.GetDirectoryName(fullDestPath).Replace("\\", "/");

                    RunADBCommand($"push \"{file}\" \"{fullDestPath}\"");
                    Debug.Log($"Pushed file: {file} to {fullDestPath}");
                }

                Debug.Log("All files successfully deployed to Android device.");
            } catch (Exception ex) {
                Debug.LogError($"An error occurred during deployment: {ex.Message}");
            }
        }

        private static void RunADBCommand(string command) {
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "adb",
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(output)) Debug.Log("ADB Output: " + output);
            if (!string.IsNullOrEmpty(error)) Debug.LogError("ADB Error: " + error);
        }
    }
}