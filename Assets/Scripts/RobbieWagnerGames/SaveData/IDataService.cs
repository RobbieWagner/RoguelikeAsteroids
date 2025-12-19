using System.Threading.Tasks;

namespace RobbieWagnerGames
{
    public interface IDataService
    {
        bool SaveData<T>(string relativePath, T data, bool encrypt = false);
        Task<bool> SaveDataAsync<T>(string relativePath, T data, bool encrypt = false);
        
        T LoadData<T>(string fullPath, T defaultData, bool saveDefaultIfMissing = false, bool isEncrypted = false);
        T LoadDataRelative<T>(string relativePath, T defaultData, bool saveDefaultIfMissing = false, bool isEncrypted = false);
        Task<T> LoadDataAsync<T>(string relativePath, T defaultData, bool saveDefaultIfMissing = false, bool isEncrypted = false);
        
        bool PurgeData();
    }
}