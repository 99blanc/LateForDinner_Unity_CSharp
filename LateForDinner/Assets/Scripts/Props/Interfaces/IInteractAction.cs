public interface IInteractAction
{
    public bool Execute(Character character);
}

public class LadderInteractAction : IInteractAction
{
    public bool Execute(Character character)
    {
        if (character is not IClimbableCharacter climbable)
            return false;

        Log.System(LocalizationKey.Interaction_Ladder);
        return true;
    }
}

public class TrayInteractAction : IInteractAction
{
    public bool Execute(Character character)
    {
        if (character is not ICarriableCharacter carriable || character.CurrentInteractable is not TrayProp trayProp)
            return false;

        Log.System(LocalizationKey.Interaction_Tray);
        return true;
    }
}

public class ItemInteractAction : IInteractAction
{
    public bool Execute(Character character)
    {
        if (character.CurrentInteractable is not ItemProp itemProp)
            return false;

        Log.System(LocalizationKey.Interaction_Item);
        return true;
    }
}
