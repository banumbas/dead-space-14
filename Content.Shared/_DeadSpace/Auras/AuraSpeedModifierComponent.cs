using Robust.Shared.GameStates;

namespace Content.Shared._DeadSpace.Auras;

/// <summary>
/// When placed on an entity with <see cref="AuraComponent"/>, 
/// applies a speed modifier to all entities within the aura.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AuraSpeedModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public float WalkModifier = 0.5f;

    [DataField, AutoNetworkedField]
    public float SprintModifier = 0.5f;
}
