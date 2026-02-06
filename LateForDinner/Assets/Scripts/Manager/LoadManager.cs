using R3;

public class LoadManager
{
    public ReadOnlyReactiveProperty<float> Progress => progress;
    private readonly ReactiveProperty<float> progress = new();
    public ReadOnlyReactiveProperty<string> Status => status;
    private readonly ReactiveProperty<string> status = new();
    public void SetProgress(float value) => progress.Value = value;
    public void SetStatus(string message) => status.Value = message;
}
