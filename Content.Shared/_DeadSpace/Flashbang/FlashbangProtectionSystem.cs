using Content.Shared.Clothing.Components;
using Content.Shared.Examine;
using Content.Shared.Verbs;

namespace Content.Shared._DeadSpace.Flashbang;

public sealed class FlashbangProtectionSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlashbangProtectionComponent, GetVerbsEvent<ExamineVerb>>(OnDetailedExamine);
    }

    private void OnDetailedExamine(EntityUid ent, FlashbangProtectionComponent component, ref GetVerbsEvent<ExamineVerb> args)
    {
        if (!HasComp<ClothingComponent>(ent))
            return;

        var iconTexture = "/Textures/Interface/VerbIcons/dot.svg.192dpi.png";

        _examine.AddHoverExamineVerb(args,
            component,
            Loc.GetString("flashbang-protection-examinable-verb-text"),
            Loc.GetString("flashbang-protection-examinable-verb-text-message", ("reduction", component.Reduction)),
            iconTexture
        );
    }
}
