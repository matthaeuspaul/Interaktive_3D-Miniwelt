public interface IItemUsable
{
    bool CanUseItem(PickUpItemInteraction item);
    bool UseItem(PickUpItemInteraction item);
    string GetUsePrompt(PickUpItemInteraction item);
}