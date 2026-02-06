using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class LocalizationManager
{
    public Localization UI { get; } = new();
    public Localization Stat { get; } = new();
    public Localization Dialogue { get; } = new();

    public class Localization
    {
        public Dictionary<string, LocalizationData> Data { get; internal set; } = new();
        public string GetText(string id) => Data.TryGetValue(id, out var v) ? v.Text : id;
    }

    public async UniTask Init() => await Managers.Data.Localization(this);
}
