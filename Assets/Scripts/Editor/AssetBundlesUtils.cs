using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace com.anotherworld.kernel {
    public static class AssetBundlesUtils {
        [MenuItem("Kernel/AssetBundles/Build All")]
        private static void BuildAllAddressables() {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null) {
                Debug.LogError("AddressableAssetSettings не найдены. Убедитесь, что Addressables настроены в вашем проекте.");
                return;
            }

            AddressableAssetSettings.BuildPlayerContent();
        }
    }
}
