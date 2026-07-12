// не ставь лицензию дедспейса я взял много кода с RMC
using Content.Shared.DeadSpace.MovementLimit;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using System.Collections.Generic;
using System.Numerics;
using Robust.Shared.IoC;
using Robust.Shared.GameObjects;

namespace Content.Client.DeadSpace.MovementLimit;

public sealed class DistanceLimitOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV | OverlaySpace.WorldSpace;

    private readonly IEntityManager _entManager;
    private readonly IPlayerManager _player;
    private readonly SharedTransformSystem _transform;
    private readonly SharedMapSystem _mapSystem;
    private readonly IMapManager _mapManager;

    public DistanceLimitOverlay(IEntityManager entManager)
    {
        _entManager = entManager;
        _player = IoCManager.Resolve<IPlayerManager>();
        _mapManager = IoCManager.Resolve<IMapManager>();
        _transform = _entManager.System<SharedTransformSystem>();
        _mapSystem = _entManager.System<SharedMapSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.Space != OverlaySpace.WorldSpaceBelowFOV)
            return;

        var player = _player.LocalEntity;
        if (player == null)
            return;

        if (!_entManager.TryGetComponent<DistanceLimitVisualsComponent>(player.Value, out var limitComp))
            return;

        if (limitComp.Origin == null || !_entManager.EntityExists(limitComp.Origin.Value))
            return;

        var originCoords = _transform.GetMapCoordinates(limitComp.Origin.Value);
        if (originCoords.MapId != args.MapId)
            return;

        DrawTileRange(args, originCoords, limitComp.MaxDistance, limitComp.Color);
    }

    private void DrawTileRange(in OverlayDrawArgs args, MapCoordinates originMap, float range, Color color)
    {
        if (!_mapManager.TryFindGridAt(originMap, out var gridUid, out var grid))
            return;

        var center = _mapSystem.CoordinatesToTile(gridUid, grid, originMap);
        var tileSize = grid.TileSize;
        var maxTiles = (int) MathF.Ceiling(range / tileSize);
        var tiles = new HashSet<Vector2i>();

        for (var x = -maxTiles; x <= maxTiles; x++)
        {
            for (var y = -maxTiles; y <= maxTiles; y++)
            {
                var distance = new Vector2(x * tileSize, y * tileSize).Length();
                if (distance > range)
                    continue;

                tiles.Add(center + new Vector2i(x, y));
            }
        }

        DrawTileBorder(args.WorldHandle, gridUid, grid, tiles, color.WithAlpha(0.8f));
    }

    private void DrawTileBorder(DrawingHandleWorld handle, EntityUid gridUid, MapGridComponent grid, HashSet<Vector2i> tiles, Color color)
    {
        if (tiles.Count == 0)
            return;

        var tileSize = grid.TileSize;
        var tileSizeVec = new Vector2(tileSize, tileSize);

        foreach (var indices in tiles)
        {
            var baseLocal = new Vector2(indices.X * tileSize, indices.Y * tileSize);
            var p00 = _transform.ToMapCoordinates(new EntityCoordinates(gridUid, baseLocal)).Position;
            var p10 = _transform.ToMapCoordinates(new EntityCoordinates(gridUid, baseLocal + new Vector2(tileSize, 0f))).Position;
            var p11 = _transform.ToMapCoordinates(new EntityCoordinates(gridUid, baseLocal + tileSizeVec)).Position;
            var p01 = _transform.ToMapCoordinates(new EntityCoordinates(gridUid, baseLocal + new Vector2(0f, tileSize))).Position;

            if (!tiles.Contains(new Vector2i(indices.X, indices.Y + 1)))
                DrawEdge(handle, p01, p11, color);
            if (!tiles.Contains(new Vector2i(indices.X, indices.Y - 1)))
                DrawEdge(handle, p00, p10, color);
            if (!tiles.Contains(new Vector2i(indices.X + 1, indices.Y)))
                DrawEdge(handle, p10, p11, color);
            if (!tiles.Contains(new Vector2i(indices.X - 1, indices.Y)))
                DrawEdge(handle, p00, p01, color);
        }
    }

    private const float OutlineThickness = 0.1f;

    private void DrawEdge(DrawingHandleWorld handle, Vector2 from, Vector2 to, Color color)
    {
        var dir = (to - from).Normalized();
        var ortho = new Vector2(-dir.Y, dir.X);

        var p00 = from - ortho * OutlineThickness / 2f;
        var p01 = to - ortho * OutlineThickness / 2f;
        var p11 = to + ortho * OutlineThickness / 2f;
        var p10 = from + ortho * OutlineThickness / 2f;

        var verts = new[] { p00, p01, p11, p10 };
        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleFan, verts, color);
    }
}
