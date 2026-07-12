using Robust.Shared.GameStates;

namespace Content.Shared.DeadSpace.Alerts;

/// <summary>
/// Relays the health alert of a linked entity (e.g. Host) to this entity (e.g. Holoparasite).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class HealthAlertRelayComponent : Component
{
    /// <summary>
    /// The entity whose health alerts are being relayed to this entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? LinkedEntity;
}
