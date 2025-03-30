using Unity.Entities;
using UnityEngine;

public class GridPositionAuthoring : MonoBehaviour {


    public int x;
    public int y;


    public class Baker : Baker<GridPositionAuthoring> {
        public override void Bake(GridPositionAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new GridPosition {
                x = authoring.x,
                y = authoring.y,
            });
        }
    }
}



public struct GridPosition : IComponentData {

    public int x;
    public int y;

}