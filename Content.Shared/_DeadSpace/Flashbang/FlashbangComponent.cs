using Robust.Shared.GameStates;
using Content.Shared.Trigger.Components.Effects;

namespace Content.Shared._DeadSpace.Flashbang;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FlashbangComponent : BaseXOnTriggerComponent
{

    [DataField]
    public float KnockdownRange = 6f;

    [DataField]
    public float StunRange = 4f;

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(4);

    [DataField]
    public TimeSpan StunDuration = TimeSpan.FromSeconds(2);

    [DataField]
    public float Probability = 1f;
}
