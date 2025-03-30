using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct ResetDOTSEventsClientSystem : ISystem {

    //[BurstCompile]
    public void OnUpdate(ref SystemState state) {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((
            RefRO<OnConnectedEvent> onConnectedEvent,
            Entity entity)
            in SystemAPI.Query<
                RefRO<OnConnectedEvent>>().WithEntityAccess()) {

            DOTSEventsMonoBehaviour.Instance.TriggerOnClientConnectedEvent(onConnectedEvent.ValueRO.connectionId);

            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
    }

}