using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


/// <summary>
/// Utility class for saving and loading any type of container or single values or arrays using ValueWrapperT> and ArrayWrapper<T>
/// </summary>
public static class FileManager
{
    /// <summary>
    /// Method to get all file names of a specific type in a directory
    /// </summary>
    public static (bool Succes, string[] Output) GetDirectoryContentNames(string directoryPath, string fileExtension = ".json")
    {
        EnsurePersistentDataPath(ref directoryPath);

        if (Directory.Exists(directoryPath))
        {
            // Get all file paths of the specified type in the directory
            string[] filePaths = Directory.GetFiles(directoryPath, "*" + fileExtension);

            // Extract file names from the file paths
            string[] fileNames = Array.ConvertAll(filePaths, Path.GetFileName);

            return (true, fileNames);
        }
        else
        {
            DebugLogger.LogWarning("Directory does not exist: " + directoryPath);
            return (false, default); // Returns false and an empty array if the directory doesn't exist
        }
    }

    /// <summary>
    /// Method to get all files and deserialize into an array of type T
    /// </summary>
    public static async Task<(bool Succes, T[])> LoadDirectoryContentAsync<T>(string directoryPath, string fileExtension = ".json")
    {
        // Get all file names with the specified extension
        (bool anyFileInDirectory, string[] fileNames) = GetDirectoryContentNames(directoryPath, fileExtension);

        // If atleast one file was found in the directory that has the correct fileExtensions
        if (anyFileInDirectory)
        {
            var tasks = fileNames.Select(file =>
                LoadInfoAsync<T>(Path.Combine(directoryPath, file))
            );

            (bool, T)[] results = await Task.WhenAll(tasks);

            T[] validResults = results.Where(r => r.Item1).Select(r => r.Item2).ToArray();
            return (validResults.Length > 0, validResults);
        }
        // No files found in directory with correct fileExtension
        else
        {
            DebugLogger.LogWarning($"No files with extension '{fileExtension}' found in directory: {directoryPath}");
            return (false, default);
        }
    }


    /// <summary>
    /// Save data using JSON serialization
    /// </summary>
    public async static Task SaveInfoAsync<T>(T saveData, string path, bool obfuscateFile = false, bool encryptFile = false)
    {
        try
        {
            EnsurePersistentDataPath(ref path);
            EnsureFileExtension(ref path);

            // Separate the directory path and the file name from the provided directoryPlusFileName string
            string directoryPath = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            // If directory path doesnt exist, create it
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Serialize the data to JSON format
            string outputData = JsonUtility.ToJson(saveData);

            if (obfuscateFile)
            {
                outputData = ObfuscationUtility.Obfuscate(outputData);
            }
            if (encryptFile)
            {
                //encrypt if marked for encryption
                outputData = await EncryptionUtility.EncryptAsync(outputData);
            }

            //write the data to the file
            await File.WriteAllTextAsync(path, outputData);

        }
        catch (Exception ex)
        {
            DebugLogger.LogError("Failed to save game data: " + ex.Message);
        }
    }

    /// <summary>
    /// Load data using JSON deserialization
    /// </summary>
    public async static Task<(bool Succes, T Value)> LoadInfoAsync<T>(string path, bool deObfuscateFile = false, bool decryptFile = false)
    {
        EnsurePersistentDataPath(ref path);
        EnsureFileExtension(ref path);

        if (File.Exists(path))
        {
            try
            {
                // Read the encrypted data from the file
                string outputData = await File.ReadAllTextAsync(path);

                if (decryptFile)
                {
                    // decrypt the data if marked for decryption
                    outputData = await EncryptionUtility.DecryptAsync(outputData);
                }
                if (deObfuscateFile)
                {
                    bool deObfuscationSucces;
                    (deObfuscationSucces, outputData) = ObfuscationUtility.DeObfuscate(outputData);

                    if (deObfuscationSucces == false)
                    {
                        return (false, default);
                    }
                }

                T loadedData = JsonUtility.FromJson<T>(outputData);
                return (true, loadedData);

            }
            catch (Exception ex)
            {
                DebugLogger.LogError("Failed to load file: " + ex.Message);
                return (false, default);
            }
        }
        else
        {
            DebugLogger.LogWarning("No file found at: " + path);
            return (false, default);
        }
    }


    /// <summary>
    /// Delete a File
    /// </summary>
    /// <returns>Whether the deletion was succesfull</returns>
    public static bool TryDeleteFile(string path)
    {
        EnsurePersistentDataPath(ref path);
        EnsureFileExtension(ref path);

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path); // Deletes the file
                return true;
            }
            else
            {
                DebugLogger.LogWarning($"File not found: {path}");
                return false;
            }
        }
        catch (IOException ex)
        {
            DebugLogger.LogError($"Failed to delete file {path}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Delete a Directory (Folder)
    /// </summary>
    /// <returns>Wether the deletion was succesfull</returns>
    public static bool TryDeleteDirectory(string directoryPath)
    {
        EnsurePersistentDataPath(ref directoryPath);

        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true); // Deletes the directory

                DebugLogger.Log($"Directory deleted: {directoryPath}");
                return true;
            }
            else
            {
                DebugLogger.LogWarning($"Directory not found: {directoryPath}");
                return false;
            }
        }
        catch (IOException ex)
        {
            DebugLogger.LogError($"Failed to delete directory {directoryPath}: {ex.Message}");
            return false;
        }
    }



    /// <summary>
    /// Ensure the file path starts with "Application.persistentDataPath".
    /// </summary>
    private static void EnsurePersistentDataPath(ref string path)
    {
        //if path doesnt start with "Application.persistentDataPath", add it, because all files are preferably located in a fixed path
        if (path.StartsWith(Application.persistentDataPath) == false)
        {
            path = Path.Combine(Application.persistentDataPath, path.TrimStart('/', '\\'));
        }
    }

    /// <summary>
    /// Ensure the file path has a valid file extension.
    /// </summary>
    private static void EnsureFileExtension(ref string path)
    {
        // if the "path" string doesnt have an extension (.json, .txt, etc) add .json automatically
        if (string.IsNullOrEmpty(Path.GetExtension(path)))
        {
            path += ".json";
        }
    }



#if UNITY_EDITOR
    private static string GetEditorPath(string path)
    {
        string folder = Path.Combine(Application.dataPath, "Editor");
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string fileName = Path.GetFileName(path);
        if (!fileName.EndsWith(".json"))
            fileName += ".json";

        return Path.Combine(folder, fileName);
    }

    /// <summary>
    /// Save data to Assets/Editor Folder using JSON serialization
    /// </summary>
    public static async Task SaveInfoToEditorAsync<T>(T data, string fileName)
    {
        string path = GetEditorPath(fileName);
        try
        {
            string json = JsonUtility.ToJson(data, true);
            await File.WriteAllTextAsync(path, json);
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"Failed to save editor file: {ex.Message}");
        }
    }

    /// <summary>
    /// Load data from Assets/Editor Folder using JSON deserialization
    /// </summary>
    public static async Task<(bool Succes, T Value)> LoadInfoFromEditorAsync<T>(string fileName)
    {
        string path = GetEditorPath(fileName);
        if (!File.Exists(path))
        {
            DebugLogger.LogWarning($"Editor file not found: {path}");
            return (false, default);
        }

        try
        {
            string json = await File.ReadAllTextAsync(path);
            T loadedData = JsonUtility.FromJson<T>(json);

            return (true, loadedData);
        }
        catch (Exception ex)
        {
            DebugLogger.LogError($"Failed to load editor file: {ex.Message}");
            return (false, default);
        }
    }
#endif
}


[System.Serializable]
public struct ValueWrapper<T>
{
    public T Value;

    public ValueWrapper(T _value)
    {
        Value = _value;
    }
}

[System.Serializable]
public struct ArrayWrapper<T>
{
    public T[] Array;

    public ArrayWrapper(T[] _values)
    {
        Array = _values;
    }
    public ArrayWrapper(int length)
    {
        Array = new T[length];
    }

    public T this[int index]
    {
        get => Array[index];
        set => Array[index] = value;
    }

    public int Length => Array?.Length ?? 0;
}