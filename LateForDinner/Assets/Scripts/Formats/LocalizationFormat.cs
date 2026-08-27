using Newtonsoft.Json;
using System.Collections.Generic;

public class LocalizationFormat
{
    [JsonProperty("locate")]
    public string Locate { get; set; } = Literal.Languages.Korean;

    [JsonProperty("translations")]
    public Dictionary<string, string> Translations { get; set; } = new Dictionary<string, string>();
}
