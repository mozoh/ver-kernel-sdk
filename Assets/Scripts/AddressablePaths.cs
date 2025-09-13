public static class KernelAddressablePaths {
#if UNITY_ANDROID
    public static string LoadPath => "{UnityEngine.Application.persistentDataPath}/AssetBundles/Custom";
#else
    public static string LoadPath => "AssetBundles/StandaloneWindows64/Custom";
#endif
}
