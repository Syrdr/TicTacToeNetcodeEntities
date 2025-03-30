using Unity.Burst;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct DestroyAfterTimeSystem : ISystem {

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((
            RefRW<DestroyAfterTime> destroyAfterTime,
            Entity entity)
            in SystemAPI.Query<
                RefRW<DestroyAfterTime>>().WithEntityAccess()) {

            destroyAfterTime.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (destroyAfterTime.ValueRO.timer < 0f) {
                entityCommandBuffer.DestroyEntity(entity);
            }
        }

        entityCommandBuffer.Playback(state.EntityManager);
    }


}