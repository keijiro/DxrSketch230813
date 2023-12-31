using Klak.Math;
using Sketch.Common;
using Sketch.MeshKit;
using System;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

using System.Threading.Tasks;

namespace Sketch {

// Configuration struct
[Serializable]
public struct StageConfig
{
    public uint2 CellCounts;
    public float CellSize;
    public float InstanceSize;

    [Tooltip("The random number seed")]
    public uint Seed;

    public int TotalInstanceCount
      => (int)(CellCounts.x * CellCounts.y);

    // Default configuration
    public static StageConfig Default()
      => new StageConfig()
        { CellCounts = 10,
          CellSize = 0.5f,
          InstanceSize = 0.5f,
          Seed = 1 };
}

// Builder implementation
[BurstCompile]
static class StageBuilder
{
    // Public entry point
    public static void Build
      (float time,
       StageConfig cfg,
       ReadOnlySpan<ShapeRef> shapes,
       Span<ShapeInstance> output)
    {
        Parallel.For(0, cfg.CellCounts.x * cfg.CellCounts.y,
                     idx => {
                      unsafe { var temp = Build_int(idx,
                                  idx % cfg.CellCounts.x,
                                  idx / cfg.CellCounts.x,
                                  time,
                                  cfg,
                                  shapes);
                      output[idx] = temp;
                                   }
                     });
        /*
        var idx = 0;
        for (var i = 0; i < cfg.CellCounts.x; i++)
        {
            for (var j = 0; j < cfg.CellCounts.y; j++)
            {
                unsafe { Build_burst(idx++, i, j, time, cfg, shapes, output); }
            }
        }
        */
    }

    unsafe public static ShapeInstance Build_int
      (int idx, int i, int j,
       float time,
       StageConfig cfg,
       ReadOnlySpan<ShapeRef> raw_shapes)
    {
        ShapeInstance temp;
        Build_burst(idx, i, j, time, cfg, raw_shapes, out temp);
        return temp;
    }

    // Bursted entry point
    [BurstCompile]
    unsafe public static void Build_burst
      (int idx, int i, int j,
       float time,
       in StageConfig cfg,
       in ReadOnlyRawSpan<ShapeRef> raw_shapes,
       out ShapeInstance output)
    {
        var shapes = raw_shapes.AsReadOnlySpan();
        var output = raw_output.AsSpan();
        var rand = new Random(cfg.Seed);

        var x = (i - (cfg.CellCounts.x - 1) * 0.5f) * cfg.CellSize;
        var z = (j - (cfg.CellCounts.y - 1) * 0.5f) * cfg.CellSize;

        var o1 = math.float2(time * 0.2f, 0);
        var np = math.float2(x, z) * 0.8f;
        var y = noise.snoise(np + o1) * 0.2f;
        y = math.max(0, y);

        var y2 = noise.snoise(np * 4);
        y *= y2 * y2 * y2 * y2 * 6;

        var rot = quaternion.RotateZ(0.4f);
        var scale = cfg.InstanceSize;

        var vy = math.mul(rot, math.float3(0, y, 0));

        var pos = math.float3(x, 0, z) + vy;

        // Random shape
        var shape = shapes[rand.NextInt(shapes.Length)];

        output = new MeshKit.ShapeInstance
          (position: pos,
           rotation: rot,
           scale: scale,
           color: 1,
           shape: shape);
    }
}

} // namespace Sketch
