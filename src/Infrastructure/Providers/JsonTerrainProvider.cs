using Labyrinths.Core.Abstractions;
using Labyrinths.Core.Models;
using Labyrinths.Core.Traversal;
using System.Text.Json;

namespace Labyrinths.Infrastructure.Providers;

public sealed class JsonTerrainProvider : ITerrainProvider
{
    private sealed record CostCell(int X, int Y, double Cost);

    private static readonly JsonSerializerOptions CaseInsensitive = new() 
    { 
        PropertyNameCaseInsensitive = true 
    }; 

    public async Task<ITraversalCostProvider> LoadAsync(string path, int mazeWidth, int mazeHeight, CancellationToken ct = default)
    {
        var json = await File.ReadAllTextAsync(path, ct); 

        var cells = JsonSerializer.Deserialize<List<CostCell>>(json, CaseInsensitive) ?? []; 
        var map = new TerrainMap(mazeWidth, mazeHeight); 
        
        foreach (var c in cells) 
        { 
            if (c.X < 0 || c.Y < 0 || c.X >= mazeWidth || c.Y >= mazeHeight)
            {
                continue;
            }

            var type = c.Cost switch 
            { 
                >= 3.0 => TerrainType.Mud, 
                >= 2.0 => TerrainType.Sand, 
                >= 1.0 => TerrainType.Normal, 
                >= 0.6 => TerrainType.Road,
                _ => TerrainType.Bonus 
            }; 

            map[c.X, c.Y] = type; 
        }

        return new TerrainCostProvider(map);
    }
}
