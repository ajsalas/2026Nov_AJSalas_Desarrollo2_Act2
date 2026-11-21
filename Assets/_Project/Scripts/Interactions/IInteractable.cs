using DungeonSurvivor.Player;

namespace DungeonSurvivor.Interactions
{

    public interface IInteractable
    {

        string InteractionPrompt { get; }

        bool CanInteract(PlayerInventory inventory);

        void Interact(PlayerController player);
    }
}

