using Sketch.MeshKit;
using UnityEngine;

namespace Sketch {

[ExecuteInEditMode]
public sealed class StageRenderer : MonoBehaviour
{
    #region Editable attributes

    [field:SerializeField]
    public StageConfig Config { get; set; } = StageConfig.Default();

    [field:SerializeField]
    public Mesh[] Shapes { get; set; }

    #endregion

    #region MonoBehaviour implementation

    void OnDisable()
    {
        _shapeCache.Destroy();
        _mesh.Destroy();
    }

    void Update()
      => ConstructMesh(true);

    #endregion

    #region Private members

    ShapeCache _shapeCache = new ShapeCache();
    TempMesh _mesh = new TempMesh();

    void ConstructMesh(bool forceUpdate)
    {
        if (!forceUpdate) return;

        _mesh.Clear();

        if (Shapes == null || Shapes.Length == 0) return;
        _shapeCache.Update(Shapes);

        var instances = ShapeInstanceBuffer.Get(Config.TotalInstanceCount);
        StageBuilder.Build(Time.time, Config, _shapeCache, instances);
        Baker.Bake(instances, _mesh.Attach(this));
    }

    #endregion
}

} // namespace Sketch
