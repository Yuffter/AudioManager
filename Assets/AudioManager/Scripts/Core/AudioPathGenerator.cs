using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AudioManger.Core
{
    public static class AudioPathGenerator
    {
        private const string SE_PATH = "Assets/AudioManager/Scripts/SE/";
        private const string BGM_PATH = "Assets/AudioManager/Scripts/BGM/";
        private const string SE_GROUP_NAME = "AudioManagerSE";
        private const string BGM_GROUP_NAME = "AudioManagerBGM";
        private static List<string> seAddressList = new();
        private static List<string> bgmAddressList = new();
        [MenuItem("Jobs/AudioManager/Update SE Paths")]
        public static void UpdateSEPaths()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            seAddressList.Clear();

            foreach (var group in settings.groups)
            {
                if (group.name != SE_GROUP_NAME)
                {
                    continue;
                }

                foreach (var entry in group.entries)
                {
                    /* mp3, wav, oggのみに限定する */
                    if (IsAudioFile(entry.AssetPath) == false)
                    {
                        Debug.LogError($"Invalid audio file in SE group: {entry.AssetPath}");
                        continue;
                    }

                    seAddressList.Add(entry.address);
                }
            }

            WriteAudioPathsToFile(seAddressList, SE_PATH, "SEPath");
        }

        [MenuItem("Jobs/AudioManager/Update BGM Paths")]
        public static void UpdateBGMPaths()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            bgmAddressList.Clear();

            foreach (var group in settings.groups)
            {
                if (group.name != BGM_GROUP_NAME)
                {
                    continue;
                }

                foreach (var entry in group.entries)
                {
                    /* mp3, wav, oggのみに限定する */
                    if (IsAudioFile(entry.AssetPath) == false)
                    {
                        Debug.LogError($"Invalid audio file in BGM group: {entry.AssetPath}");
                        continue;
                    }

                    bgmAddressList.Add(entry.address);
                }
            }

            WriteAudioPathsToFile(bgmAddressList, BGM_PATH, "BGMPath");
        }

        [MenuItem("Jobs/AudioManager/Update All Audio Paths")]
        public static void UpdateAllAudioPaths()
        {
            UpdateSEPaths();
            UpdateBGMPaths();
        }

        private static void WriteAudioPathsToFile(List<string> paths, string folderPath, string fileName)
        {
            string filePath = System.IO.Path.Combine(folderPath, fileName + ".cs");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var sb = new StringBuilder();
            sb.AppendLine("// Auto-generated audio paths");
            sb.Append($"public static class {fileName}");
            sb.AppendLine(" {");
            foreach (var path in paths)
            {
                sb.AppendLine($"    public static readonly string {path} = \"{path}\";");
            }
            sb.AppendLine("}");
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// オーディオファイルかどうかを判定する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsAudioFile(string path)
        {
            string extension = System.IO.Path.GetExtension(path).ToLower();
            return extension == ".mp3" || extension == ".wav" || extension == ".ogg";
        }
    }
}