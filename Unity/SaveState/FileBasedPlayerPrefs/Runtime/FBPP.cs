using System;
using System.Text;
using System.IO;
using UnityEngine;

public static class FBPP {

    private class FBPPInitException : Exception {
        public FBPPInitException(string message) : base(message) { }
    }

    private const string INIT_EXCEPTION_MESSAGE = "Error, you must call FBPP.Start(FBPPConfig config) before trying to get or set saved data.";

    private static FBPPConfig _config;

    public static bool ShowInitWarning = true;

    private static FBPPFileModel _latestData;
    private static StringBuilder _sb = new StringBuilder();

    const string String_Empty = "";

    #region Init

    static FBPP() {
        var config = new FBPPConfig() {
            SaveFileName = "savedata.txt",
            AutoSaveData = false,
            ScrambleSaveData = false,
            //EncryptionSecret = "fourtreasures",
            //SaveFilePath = "my/explicit/savefile/path"
        };
        FBPP.Start(config);
    }

    public static void Start(FBPPConfig config) {
        _config = config;
        _latestData = GetSaveFile();
    }

    private static void CheckForInit() {
        if (_config == null) {
            throw new FBPPInitException(INIT_EXCEPTION_MESSAGE);
        }
    }

    #endregion


    #region Public Get, Set and util

    public static void SetString(string key, string value = String_Empty) {
        AddDataToSaveFile(key, value);
    }

    public static string GetString(string key, string defaultValue = String_Empty) {
        return (string)GetDataFromSaveFile(key, defaultValue);
    }

    public static void SetInt(string key, int value = default(int)) {
        AddDataToSaveFile(key, value);
    }

    public static int GetInt(string key, int defaultValue = default(int)) {
        return (int)GetDataFromSaveFile(key, defaultValue);
    }

    public static void SetFloat(string key, float value = default(float)) {
        AddDataToSaveFile(key, value);
    }

    public static float GetFloat(string key, float defaultValue = default(float)) {
        return (float)GetDataFromSaveFile(key, defaultValue);
    }

    public static void SetBool(string key, bool value = default(bool)) {
        AddDataToSaveFile(key, value);
    }

    public static bool GetBool(string key, bool defaultValue = default(bool)) {
        return (bool)GetDataFromSaveFile(key, defaultValue);
    }

    public static bool HasKey(string key) {
        return GetSaveFile().HasKey(key);
    }

    public static bool HasKeyForString(string key) {
        return GetSaveFile().HasKeyFromObject(key, string.Empty);
    }

    public static bool HasKeyForInt(string key) {
        return GetSaveFile().HasKeyFromObject(key, default(int));
    }

    public static bool HasKeyForFloat(string key) {
        return GetSaveFile().HasKeyFromObject(key, default(float));
    }

    public static bool HasKeyForBool(string key) {
        return GetSaveFile().HasKeyFromObject(key, default(bool));
    }

    public static void DeleteKey(string key) {
        GetSaveFile().DeleteKey(key);
        SaveSaveFile();
    }

    public static void DeleteString(string key) {
        GetSaveFile().DeleteString(key);
        SaveSaveFile();
    }

    public static void DeleteInt(string key) {
        GetSaveFile().DeleteInt(key);
        SaveSaveFile();
    }

    public static void DeleteFloat(string key) {
        GetSaveFile().DeleteFloat(key);
        SaveSaveFile();
    }

    public static void DeleteBool(string key) {
        GetSaveFile().DeleteBool(key);
        SaveSaveFile();
    }

    public static void DeleteAll() {
        WriteToSaveFile(JsonUtility.ToJson(new FBPPFileModel()));
        _latestData = new FBPPFileModel();
    }

    public static void OverwriteLocalSaveFile(string data) {
        WriteToSaveFile(data);
        _latestData = null;
        _latestData = GetSaveFile();
    }


    #endregion



    #region Read data

    private static FBPPFileModel GetSaveFile() {
        CheckForInit();
        CheckSaveFileExists();
        if (_latestData == null) {
            var saveFileText = File.ReadAllText(GetSaveFilePath());
            if (_config.ScrambleSaveData) {
                saveFileText = DataScrambler(saveFileText);
            }
            try {
                _latestData = JsonUtility.FromJson<FBPPFileModel>(saveFileText);
            }
            catch (ArgumentException e) {
                Debug.LogException(new Exception("FBPP Error loading save file: " + e.Message));
                if (_config.OnLoadError != null) {
                    _config.OnLoadError.Invoke();
                }
                else {
                    DeleteAll();
                }
            }
        }
        return _latestData;
    }

    public static string GetSaveFilePath() {
        CheckForInit();
        return Path.Combine(_config.GetSaveFilePath(), _config.SaveFileName);
    }



    public static string GetSaveFileAsJson() {
        CheckForInit();
        CheckSaveFileExists();
        return JsonUtility.ToJson(GetSaveFile());
    }

    private static object GetDataFromSaveFile(string key, object defaultValue) {
        return GetSaveFile().GetValueForKey(key, defaultValue);
    }

    #endregion


    #region write data

    private static void AddDataToSaveFile(string key, object value) {
        CheckForInit();
        GetSaveFile().UpdateOrAddData(key, value);
        SaveSaveFile();
    }

    public static void Save() {
        CheckForInit();
        SaveSaveFile(true);
    }

    private static void SaveSaveFile(bool manualSave = false) {
        if (_config.AutoSaveData || manualSave) {
            WriteToSaveFile(JsonUtility.ToJson(GetSaveFile()));
        }
    }
    private static void WriteToSaveFile(string data) {
        string path = GetSaveFilePath();
        string extension = Path.GetExtension(path);
        string pathNew = path.Substring(0, path.Length - extension.Length) + "_new" + extension;
        string pathOld = path.Substring(0, path.Length - extension.Length) + "_old" + extension;

        try {
            // Save into new file.
            var tw = new StreamWriter(pathNew);
            if (_config.ScrambleSaveData) {
                data = DataScrambler(data);
            }
            tw.Write(data);
            tw.Close();

            // Check that new file can be loaded.
            var saveFileText = File.ReadAllText(pathNew);
            if (_config.ScrambleSaveData) {
                saveFileText = DataScrambler(saveFileText);
            }
            FBPPFileModel loadedModel = JsonUtility.FromJson<FBPPFileModel>(saveFileText);

            // Check that loaded data is identical to what we just saved.
            if (_latestData != null && !loadedModel.Equals(_latestData)) {
                throw new Exception("Loaded data is not identical to just saved data.");
            }

            // Backup existing file and replace existing file with new file.
            if (File.Exists(path))
                File.Replace(pathNew, path, pathOld);
            else
                File.Move(pathNew, path);
        }
        catch (Exception e) {
            Debug.LogException(new Exception("Error saving data: " + e.Message));
        }
    }

    #endregion


    #region File Utils

    private static void CheckSaveFileExists() {
        if (!DoesSaveFileExist()) {
            CreateNewSaveFile();
        }
    }

    private static bool DoesSaveFileExist() {
        return File.Exists(GetSaveFilePath());
    }

    private static void CreateNewSaveFile() {
        WriteToSaveFile(JsonUtility.ToJson(new FBPPFileModel()));
    }

    private static string DataScrambler(string data) {
        _sb.Remove(0, _sb.Length);

        for (int i = 0; i < data.Length; i++) {
            _sb.Append((char)(data[i] ^ _config.EncryptionSecret[i % _config.EncryptionSecret.Length]));
        }
        return _sb.ToString();
    }

    #endregion
}


