public interface IInteractAction
{
    public void Execute(PlayableCharacter player);
}

public class NPCInteractAction : IInteractAction
{
    public void Execute(PlayableCharacter player)
        => Log.System(LocalizationKey.Interaction_NPC);
}

public class QuestInteractAction : IInteractAction
{
    public void Execute(PlayableCharacter player)
        => Log.System(LocalizationKey.Interaction_Quest);
}

public class ShopInteractAction : IInteractAction
{
    public void Execute(PlayableCharacter player)
        => Log.System(LocalizationKey.Interaction_Shop);
}

public class SaveInteractAction : IInteractAction
{
    public void Execute(PlayableCharacter player)
        => Log.System(LocalizationKey.Interaction_Save);
}

public class EventInteractAction : IInteractAction
{
    public void Execute(PlayableCharacter player)
        => Log.System(LocalizationKey.Interaction_Event);
}

public class StageProgressInteractAction : IInteractAction
{
    public void Execute(PlayableCharacter player)
        => Log.System(LocalizationKey.Interaction_StageProgress);
}

public class DoorInteractAction : IInteractAction
{
    public void Execute(PlayableCharacter player)
        => Log.System(LocalizationKey.Interaction_Door);
}

public class TrayHolderInteractAction : IInteractAction
{
    public void Execute(PlayableCharacter player)
        => Log.System(LocalizationKey.Interaction_TrayHolder);
}

public class TrayInteractAction : IInteractAction
{
    public void Execute(PlayableCharacter player)
        => Log.System(LocalizationKey.Interaction_Tray);
}

public class DiningTableInteractAction : IInteractAction
{
    public void Execute(PlayableCharacter player)
        => Log.System(LocalizationKey.Interaction_DiningTable);
}
