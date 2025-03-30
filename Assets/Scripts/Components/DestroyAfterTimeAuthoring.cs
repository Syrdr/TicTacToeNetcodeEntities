using Unity.Entities;
using UnityEngine;

public class DestroyAfterTimeAuthoring : MonoBehaviour {


    public float timer;
    

    public class Baker : Baker<DestroyAfterTimeAuthoring> {

        public override void Bake(DestroyAfterTimeAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DestroyAfterTime {
                timer = authoring.timer
            });
        }
    }
}



public struct DestroyAfterTime : IComponentData {

    public float timer;

}