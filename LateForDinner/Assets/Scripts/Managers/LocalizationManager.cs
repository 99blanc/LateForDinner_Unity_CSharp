using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalizationManager
{
    private Dictionary<string, string> _caches = new Dictionary<string, string>();
    private Dictionary<string, string> _overrides = new Dictionary<string, string>();

    public async UniTask InitAsync()
    {
        await SyncAsync();

        RefreshAsync();
    }

    private async UniTask SyncAsync()
    {
        string dir = GetDirectory();
        string language = Managers.Config?.Settings?.Access?.language ?? Literal.Languages.Korean;
        string path = Path.Combine(dir, $"Localization_{language.ToEnglish()}.json");
        string[] files = Directory.GetFiles(dir, "*.json");

        foreach (string file in files)
        {
            try
            {
                string json = await File.ReadAllTextAsync(file);
                var tempFormat = JsonConvert.DeserializeObject<LocalizationFormat>(json);

                if (tempFormat != null && tempFormat.Locate == language)
                {
                    path = file;
                    break;
                }
            }
            catch
            {
                // DESC ::: 예외 발생 시 무시
            }
        }

        try
        {
            LocalizationFormat file = null;

            if (File.Exists(path))
            {
                string json = await File.ReadAllTextAsync(path);
                file = JsonConvert.DeserializeObject<LocalizationFormat>(json);
            }

            if (file == null || file.Translations == null)
            {
                file = new LocalizationFormat
                {
                    Locate = language,
                    Translations = GetLocalizations()
                };

                await SaveAsync(path, file);
            }
            else
            {
                _overrides = file.Translations;
                var defaultData = GetLocalizations();
                int addedCount = 0;

                foreach (var pair in defaultData)
                {
                    if (!_overrides.ContainsKey(pair.Key))
                    {
                        _overrides[pair.Key] = pair.Value;
                        addedCount++;
                    }
                }

                if (addedCount > 0)
                {
                    file.Locate = language;
                    file.Translations = _overrides;

                    await SaveAsync(path, file);
                }
            }

            _overrides = file.Translations;
        }
        catch
        {
            // DESC ::: 예외 발생 시 무시
        }
    }

    private async UniTask SaveAsync(string path, LocalizationFormat file)
    {
        try
        {
            string json = JsonConvert.SerializeObject(file, Formatting.Indented);
            await File.WriteAllTextAsync(path, json);
        }
        catch
        {
            // DESC ::: 예외 발생 시 무시
        }
    }

    public void RefreshAsync()
    {
        _caches.Clear();
        _caches = GetLocalizations();
    }

    private Dictionary<string, string> GetLocalizations()
    {
        var dict = new Dictionary<string, string>();

        if (Managers.Data != null)
        {
            foreach (var data in Managers.Data.Localization.Values)
                dict[data.Key] = data.Text;
        }

        return dict;
    }

    private string GetDirectory()
    {
        string dir = Path.Combine(Application.persistentDataPath, "Localizations");

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        return dir;
    }

    public List<string> GetLanguages()
    {
        var languages = new List<string>();
        string dir = GetDirectory();
        string[] files = Directory.GetFiles(dir, "*.json");

        foreach (string file in files)
        {
            try
            {
                string json = File.ReadAllText(file);
                var format = JsonConvert.DeserializeObject<LocalizationFormat>(json);

                if (format != null && !string.IsNullOrEmpty(format.Locate))
                {
                    if (!languages.Contains(format.Locate))
                        languages.Add(format.Locate);
                }
            }
            catch
            {
                // DESC ::: 예외 발생 시 무시
            }
        }

        return languages;
    }

    public string Get(string id)
    {
        if (_overrides.TryGetValue(id, out var text))
            return text;

        if (_caches.TryGetValue(id, out text))
            return text;

        return id;
    }

    public string Get(Localization id)
        => Get(id.ToString());
}
