using Robust.Shared.GameStates;

namespace Content.Shared._DeadSpace.Flashbang;

[RegisterComponent, NetworkedComponent]
public sealed partial class FlashbangProtectionComponent : Component
{
    [DataField]
    public float Reduction = 0f;
}
