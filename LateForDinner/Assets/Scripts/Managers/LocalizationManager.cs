using Cysharp.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

public class LocalizationManager
{
    private Dictionary<string, string> _caches = new Dictionary<string, string>();
    private Dictionary<string, string> _overrides = new Dictionary<string, string>();

    public async UniTask InitAsync()
    {
        await SyncAsync();
        RefreshAsync();
        Log.Info(LocalizationKey.Log_Localization_LoadedSuccessfully);
    }

    private async UniTask SyncAsync()
    {
        string dir = Literal.Folders.Localizations.GetDirectory();
        string language = Managers.Config?.Option?.Access?.language ?? Literal.Languages.Korean;
        string path = Path.Combine(dir, ZString.Concat(Literal.Files.Localization, "_", language.ToEnglish().ToLower(), Literal.Extensions.Json));
        string[] files = Directory.GetFiles(dir, "*.json");

        for (int index = 0; index < files.Length; index++)
        {
            string file = files[index];

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
                Log.Warning(LocalizationKey.Log_Localization_FileReadFailed, Path.GetFileName(file));
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
                int changeCount = 0;

                foreach (var pair in defaultData)
                {
                    if (_overrides.ContainsKey(pair.Key))
                        continue;

                    _overrides[pair.Key] = pair.Value;
                    changeCount++;
                }

                var keysToRemove = new List<string>();

                foreach (var key in _overrides.Keys)
                {
                    if (!defaultData.ContainsKey(key))
                        keysToRemove.Add(key);
                }

                foreach (var key in keysToRemove)
                {
                    _overrides.Remove(key);
                    changeCount++;
                }

                if (changeCount > 0)
                {
                    file.Locate = language;
                    file.Translations = _overrides;
                    await SaveAsync(path, file);
                    Log.System(LocalizationKey.Log_Localization_Synced, changeCount);
                }
            }

            _overrides = file.Translations;
        }
        catch
        {
            Log.Error(LocalizationKey.Log_Localization_SyncFailed);
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
            Log.Error(LocalizationKey.Log_Localization_SaveFailed, Path.GetFileName(path));
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

        if (IsDataModelNull())
            return dict;

        foreach (var data in Managers.Data.Localization.Values)
            dict[data.Key] = data.Text;

        return dict;
    }

    public List<string> GetLanguages()
    {
        var languages = new List<string>();
        string dir = Literal.Folders.Localizations.GetDirectory();
        string[] files = Directory.GetFiles(dir, "*.json");

        for (int index = 0; index < files.Length; index++)
        {
            string file = files[index];

            try
            {
                string json = File.ReadAllText(file);
                var format = JsonConvert.DeserializeObject<LocalizationFormat>(json);

                if (IsLocalizationFormatInvalid(format))
                    continue;

                if (!languages.Contains(format.Locate))
                    languages.Add(format.Locate);
            }
            catch
            {
                Log.Warning(LocalizationKey.Log_Localization_LanguageFileParseFailed, Path.GetFileName(file));
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

    public string Get(LocalizationKey id)
        => Get(id.ToString());

    public string Get<T1>(LocalizationKey id, T1 arg1)
        => FormatText(id, text => ZString.Format(text, arg1));

    public string Get<T1, T2>(LocalizationKey id, T1 arg1, T2 arg2)
        => FormatText(id, text => ZString.Format(text, arg1, arg2));

    public string Get<T1, T2, T3>(LocalizationKey id, T1 arg1, T2 arg2, T3 arg3)
        => FormatText(id, text => ZString.Format(text, arg1, arg2, arg3));

    public string Get(LocalizationKey id, params object[] args)
    {
        string text = Get(id);

        if (HasNoArguments(args))
            return text;

        try
        {
            return ZString.Format(text, args);
        }
        catch
        {
            return text;
        }
    }

    private string FormatText(LocalizationKey id, Func<string, string> formatAction)
    {
        string text = Get(id);

        try
        {
            return formatAction(text);
        }
        catch
        {
            return text;
        }
    }

    public async UniTask ChangeLanguageAsync(string language)
    {
        await SyncAsync();
        RefreshAsync();
        Log.Info(LocalizationKey.Log_Localization_LoadedSuccessfully);
    }

    private bool IsDataModelNull()
        => Managers.Data == null;

    private bool IsLocalizationFormatInvalid(LocalizationFormat format)
        => format == null || string.IsNullOrEmpty(format.Locate);

    private bool HasNoArguments(object[] args)
        => args == null || args.Length == 0;
}
