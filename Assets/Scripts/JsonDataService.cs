using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class JsonDataService : IDataService
{
    public bool IsFileExists(string RelativePath)
    {
        string path = Application.persistentDataPath + RelativePath;
        return File.Exists(path);
    }
    public T LoadData<T>(string RelativePath)
    {
        string path = Application.persistentDataPath + RelativePath;
        if (!File.Exists(path))
        {
            //Debug.Log("no file");
        }
        try
        {
            T data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            throw e;
        }
    }

    public bool SaveData<T>(string RelavivePath, T Data)
    {
        string path = Application.persistentDataPath + RelavivePath;
        try
        {
            if (File.Exists(path))
            {
                //Debug.Log("Data exists");
                File.Delete(path);
            }
            else
            {
                //Debug.Log("Create new file");
            }
            using FileStream stream = File.Create(path);
            stream.Close();
            File.WriteAllText(path, JsonConvert.SerializeObject(Data));
            return true;
        }
        catch (Exception)
        {
            //Debug.Log($"Cant save data {e.Message}");
            return false;
        }
    }
}
