using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[UpdateAfter(typeof(GoInGameClientSystem))]
partial struct GameClientSystem : ISystem {

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<EntitiesReferences>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        // OnConnectedEvent
        foreach (RefRO<OnConnectedEvent> onConnectedEvent in SystemAPI.Query<RefRO<OnConnectedEvent>>()) {

            RefRW<GameClientData> gameClientData = SystemAPI.GetSingletonRW<GameClientData>();
            if (onConnectedEvent.ValueRO.connectionId == 1) {
                gameClientData.ValueRW.localPlayerType = PlayerType.Cross;
            } else {
                gameClientData.ValueRW.localPlayerType = PlayerType.Circle;
            }
            Debug.Log("Assigned: " + gameClientData.ValueRW.localPlayerType);
        }

        // GameStartedRpc
        foreach ((
            RefRO<GameStartedRpc> gameStartedRpc,
            Entity entity)
            in SystemAPI.Query<RefRO<GameStartedRpc>>().WithEntityAccess()) {

            Debug.Log("GameStartedRpc");
            DOTSEventsMonoBehaviour.Instance.TriggerOnGameStarted();

            entityCommandBuffer.DestroyEntity(entity);
        }

        // GameWinRpc
        foreach ((
            RefRO<GameWinRpc> gameWinRpc,
            Entity entity)
            in SystemAPI.Query<RefRO<GameWinRpc>>().WithEntityAccess()) {

            Debug.Log("GameWinRpc");
            DOTSEventsMonoBehaviour.Instance.TriggerOnGameWin(gameWinRpc.ValueRO.winningPlayerType);

            GameClientData gameClientData = SystemAPI.GetSingleton<GameClientData>();
            if (gameWinRpc.ValueRO.winningPlayerType == gameClientData.localPlayerType) {
                entityCommandBuffer.Instantiate(entitiesReferences.winSfxPrefabEntity);
            } else {
                entityCommandBuffer.Instantiate(entitiesReferences.loseSfxPrefabEntity);
            }

            entityCommandBuffer.DestroyEntity(entity);
        }

        // RematchRpc
        foreach ((
            RefRO<RematchRpc> rematchRpc,
            Entity entity)
            in SystemAPI.Query<RefRO<RematchRpc>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess()) {

            Debug.Log("RematchRpc");
            DOTSEventsMonoBehaviour.Instance.TriggerOnGameRematch();

            entityCommandBuffer.DestroyEntity(entity);
        }

        // GameTieRpc
        foreach ((
            RefRO<GameTieRpc> gameTieRpc,
            Entity entity)
            in SystemAPI.Query<RefRO<GameTieRpc>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess()) {

            Debug.Log("GameTieRpc");
            DOTSEventsMonoBehaviour.Instance.TriggerOnGameTie();

            entityCommandBuffer.DestroyEntity(entity);
        }

        // ClickedOnGridPositionRpc
        foreach ((
            RefRO<ClickedOnGridPositionRpc> clickedOnGridPositionRpc,
            Entity entity)
            in SystemAPI.Query<RefRO<ClickedOnGridPositionRpc>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess()) {

            Debug.Log("ClickedOnGridPositionRpc");

            entityCommandBuffer.Instantiate(entitiesReferences.placeSfxPrefabEntity);

            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
    }
}