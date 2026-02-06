using Token.LANGUAGE;

public class LocalizationData
{
    public string ID { get; set; }
    public string English { get; set; }
    public string Japanese { get; set; }
    public string Korean { get; set; }
    public string Text => Managers.Config.value.language switch
    {
        Language.JAPANESE => Japanese,
        Language.KOREAN => Korean,
        _ => English
    };
}
