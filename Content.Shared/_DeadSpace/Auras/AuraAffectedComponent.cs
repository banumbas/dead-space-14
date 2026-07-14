using Robust.Shared.GameStates;

namespace Content.Shared._DeadSpace.Auras;

/// <summary>
/// Added dynamically by AuraSystem when an entity overlaps with one or more auras.
/// Handled fully in prediction.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(AuraSystem))]
public sealed partial class AuraAffectedComponent : Component
{
    /// <summary>
    /// Auras currently affecting this entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> ActiveAuras = new();

    /// <summary>
    /// The visual entity spawned to represent the aura effect (handled purely locally to prevent prediction dupes).
    /// </summary>
    [DataField]
    public EntityUid? VisualEntity;
}
