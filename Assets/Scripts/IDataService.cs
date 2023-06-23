public interface IDataService
{
    bool SaveData<T>(string RelavivePath, T Data);
    bool IsFileExists(string RelavivePath);

    T LoadData<T>(string RelativePath);
}
