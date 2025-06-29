using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets;

public static class AddressableLoad {
    [MenuItem("AudioManager/Addressable/Generate Addressable Paths")]
    public static void Export()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        foreach (var group in settings.groups)
        {
            foreach (var entry in group.entries)
            {
                Debug.Log($"Group: {group.name}, Address: {entry.address}, Path: {entry.AssetPath}");
            }
        }
    }
}
