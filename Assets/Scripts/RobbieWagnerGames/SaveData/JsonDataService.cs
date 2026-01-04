using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace RobbieWagnerGames.Utilities.SaveData
{
    public class JsonDataService : MonoBehaviourSingleton<JsonDataService>, IDataService
    {
        public bool SaveData<T>(string relativePath, T data, bool encrypt = false)
        {
            string path = CreateValidDataPath(relativePath);
            Debug.Log(path);
            return SaveDataInternal(path, data, encrypt);
        }

        public async Task<bool> SaveDataAsync<T>(string relativePath, T data, bool encrypt = false)
        {
            string path = CreateValidDataPath(relativePath);
            return await Task.Run(() => SaveDataInternal(path, data, encrypt));
        }

        private bool SaveDataInternal<T>(string fullPath, T data, bool encrypt)
        {
            try
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                
                using (FileStream stream = File.Create(fullPath))
                {
                    // Empty creation just to ensure file exists
                }
                
                string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(fullPath, jsonData);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save data: {e.Message}");
                return false;
            }
        }

        public T LoadData<T>(string fullPath, T defaultData, bool saveDefaultIfMissing = false, bool isEncrypted = false)
        {
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"File not found at path: {fullPath}. Returning default data.");
                
                if (saveDefaultIfMissing)
                {
                    SaveDataInternal(fullPath, defaultData, isEncrypted);
                }
                
                return defaultData;
            }

            try
            {
                string jsonData = File.ReadAllText(fullPath);
                return JsonConvert.DeserializeObject<T>(jsonData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load data: {e.Message}");
                return defaultData;
            }
        }

        public T LoadDataRelative<T>(string relativePath, T defaultData, bool saveDefaultIfMissing = false, bool isEncrypted = false)
        {
            string path = CreateValidDataPath(relativePath);
            return LoadData(path, defaultData, saveDefaultIfMissing, isEncrypted);
        }

        public async Task<T> LoadDataAsync<T>(string relativePath, T defaultData, bool saveDefaultIfMissing = false, bool isEncrypted = false)
        {
            string path = CreateValidDataPath(relativePath);
            return await Task.Run(() => LoadData(path, defaultData, saveDefaultIfMissing, isEncrypted));
        }

        public bool DeleteData(string relativePath)
        {
            string path = CreateValidDataPath(relativePath);
            return DeleteDataInternal(path);
        }

        private bool DeleteDataInternal(string fullPath)
        {
            try
            {
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning($"File not found at path: {fullPath}. Nothing to delete.");
                    return true;
                }
                
                File.Delete(fullPath);
                Debug.Log($"Successfully deleted file at path: {fullPath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete data: {e.Message}");
                return false;
            }
        }

        public bool PurgeData()
        {
            string path = StaticGameStats.SaveData.PersistentDataPath;
            
            try
            {
                if (!Directory.Exists(path))
                {
                    Debug.LogWarning("No data directory found to purge.");
                    return true;
                }

                DirectoryInfo directory = new DirectoryInfo(path);
                
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }

                foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                {
                    subDirectory.Delete(true);
                }

                Debug.Log("Successfully purged all save data.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to purge data: {e.Message}");
                return false;
            }
        }

        private string CreateValidDataPath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentException("Path cannot be null or empty", nameof(relativePath));
            }

            string normalizedPath = relativePath.Trim();
            
            if (!normalizedPath.StartsWith("/"))
            {
                normalizedPath = "/" + normalizedPath;
            }
            
            if (!normalizedPath.EndsWith(".json"))
            {
                normalizedPath += ".json";
            }

            return StaticGameStats.SaveData.PersistentDataPath + normalizedPath;
        }
    }
}