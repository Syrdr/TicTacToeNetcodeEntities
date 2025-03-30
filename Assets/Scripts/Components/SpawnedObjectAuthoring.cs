using Unity.Entities;
using UnityEngine;

public class SpawnedObjectAuthoring : MonoBehaviour {

    public class Baker : Baker<SpawnedObjectAuthoring> {


        public override void Bake(SpawnedObjectAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SpawnedObject());
        }
    }
}


public struct SpawnedObject : IComponentData {


}