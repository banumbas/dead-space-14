using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;
using Robust.Shared.Network;
using Robust.Shared.Containers;
using System.Linq;
using System.Collections.Generic;
using Content.Shared.Mobs.Components;

namespace Content.Shared._DeadSpace.Auras;

public sealed class AuraSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    private readonly Dictionary<EntityUid, HashSet<EntityUid>> _currentOverlaps = new();
    private readonly HashSet<EntityUid> _inRangeBuffer = new();

    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<MovementSpeedModifierComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<AuraAffectedComponent, ComponentShutdown>(OnAffectedShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var set in _currentOverlaps.Values)
        {
            set.Clear();
        }

        var query = EntityQueryEnumerator<AuraComponent, TransformComponent>();
        while (query.MoveNext(out var auraUid, out var aura, out var xform))
        {
            if (xform.MapID == Robust.Shared.Map.MapId.Nullspace || _container.IsEntityInContainer(auraUid))
                continue;

            _inRangeBuffer.Clear();
            _lookup.GetEntitiesInRange(xform.MapID, xform.MapPosition.Position, aura.Range, _inRangeBuffer, LookupFlags.Dynamic | LookupFlags.Sundries);

            foreach (var ent in _inRangeBuffer)
            {
                if (ent == auraUid || !HasComp<MobStateComponent>(ent) || !HasComp<PhysicsComponent>(ent)) continue;
                if (aura.IgnoredEntity != null && GetEntity(aura.IgnoredEntity.Value) == ent) continue;

                if (!_currentOverlaps.TryGetValue(ent, out var set))
                {
                    set = new HashSet<EntityUid>();
                    _currentOverlaps[ent] = set;
                }
                set.Add(auraUid);
            }
        }

        var affectedQuery = EntityQueryEnumerator<AuraAffectedComponent>();
        while (affectedQuery.MoveNext(out var ent, out var affected))
        {
            if (!_currentOverlaps.TryGetValue(ent, out var newAuras) || newAuras.Count == 0)
            {
                RemComp<AuraAffectedComponent>(ent);
                _movementSpeed.RefreshMovementSpeedModifiers(ent);
                continue;
            }

            if (!affected.ActiveAuras.SetEquals(newAuras))
            {
                affected.ActiveAuras.Clear();
                foreach (var a in newAuras) affected.ActiveAuras.Add(a);
                    
                Dirty(ent, affected);
                HandleVisuals(ent, affected, newAuras);
                _movementSpeed.RefreshMovementSpeedModifiers(ent);
            }
        }

        foreach (var (ent, auras) in _currentOverlaps)
        {
            if (auras.Count == 0 || HasComp<AuraAffectedComponent>(ent))
                continue;

            var affected = EnsureComp<AuraAffectedComponent>(ent);
            affected.ActiveAuras.Clear();
            foreach (var a in auras) affected.ActiveAuras.Add(a);
                
            Dirty(ent, affected);
            HandleVisuals(ent, affected, auras);
            _movementSpeed.RefreshMovementSpeedModifiers(ent);
        }
    }

    private void HandleVisuals(EntityUid ent, AuraAffectedComponent affected, HashSet<EntityUid> auras)
    {
        string? visualProto = null;
        foreach (var auraUid in auras)
        {
            if (TryComp<AuraComponent>(auraUid, out var aura) && !string.IsNullOrEmpty(aura.VisualEffectPrototype))
            {
                visualProto = aura.VisualEffectPrototype;
                break;
            }
        }

        if (visualProto != null && affected.VisualEntity == null)
        {
            var visual = Spawn(visualProto, Transform(ent).Coordinates);
            _transform.SetParent(visual, ent);
            affected.VisualEntity = visual;
            Dirty(ent, affected);
        }
        else if (visualProto == null && affected.VisualEntity != null)
        {
            QueueDel(affected.VisualEntity.Value);
            affected.VisualEntity = null;
            Dirty(ent, affected);
        }
    }

    private void OnAffectedShutdown(EntityUid uid, AuraAffectedComponent component, ComponentShutdown args)
    {
        if (component.VisualEntity != null)
            QueueDel(component.VisualEntity.Value);
    }

    private void OnRefreshSpeed(EntityUid uid, MovementSpeedModifierComponent component, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<AuraAffectedComponent>(uid, out var affected))
            return;

        float walkMod = 1f;
        float sprintMod = 1f;

        foreach (var auraUid in affected.ActiveAuras)
        {
            if (TryComp<AuraSpeedModifierComponent>(auraUid, out var modifier))
            {
                walkMod *= modifier.WalkModifier;
                sprintMod *= modifier.SprintModifier;
            }
        }

        args.ModifySpeed(walkMod, sprintMod);
    }
}
