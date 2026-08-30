public interface IInteractAction
{
    public void Execute(Character character);
}

public class LadderInteractAction : IInteractAction
{
    public void Execute(Character character)
        => Log.System(LocalizationKey.Interaction_Ladder);
}

public class TrayInteractAction : IInteractAction
{
    public void Execute(Character character)
    {
        if (character is not ICarriableCharacter carriable)
            return;

        Log.System(LocalizationKey.Interaction_Tray);
    }
}
