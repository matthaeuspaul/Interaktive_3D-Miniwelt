
using UnityEngine;

public interface IInteractable
{
    bool CanInteract();
    bool Interact(Interactor interactor);
    string GetInteractionPrompt(); 
}