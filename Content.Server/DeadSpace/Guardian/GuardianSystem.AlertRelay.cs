using Content.Shared.DeadSpace.Alerts;

namespace Content.Server.Guardian;

public sealed partial class GuardianSystem
{
    partial void OnGuardianCreated(EntityUid guardian, EntityUid host)
    {
        if (TryComp<HealthAlertRelayComponent>(guardian, out var relayComp))
        {
            relayComp.LinkedEntity = host;
            Dirty(guardian, relayComp);
        }
    }

    private void OnGuardianStartup(EntityUid uid, GuardianComponent component, ComponentStartup args)
    {
        if (TryComp<HealthAlertRelayComponent>(uid, out var relay))
        {
            relay.LinkedEntity = component.Host;
            Dirty(uid, relay);
        }
    }
}
