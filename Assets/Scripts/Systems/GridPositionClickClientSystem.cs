using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct GridPositionClickClientSystem : ISystem {

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        if (Input.GetMouseButtonDown(0)) {
            Debug.Log("Click!");
            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

            float3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (collisionWorld.CastRay(new RaycastInput {
                    Start = mouseWorldPosition,
                    End = mouseWorldPosition + new float3(0, 0, 100),
                    Filter = CollisionFilter.Default,
                }, out Unity.Physics.RaycastHit raycastHit)) {

                if (SystemAPI.HasComponent<GridPosition>(raycastHit.Entity)) {
                    GridPosition gridPosition = SystemAPI.GetComponent<GridPosition>(raycastHit.Entity);
                    Debug.Log("Clicked " + gridPosition.x + ", " + gridPosition.y);

                    GameClientData gameClientData = SystemAPI.GetSingleton<GameClientData>();
                    Entity rpcEntity = state.EntityManager.CreateEntity(typeof(ClickedOnGridPositionRpc), typeof(SendRpcCommandRequest));
                    state.EntityManager.SetComponentData(rpcEntity, new ClickedOnGridPositionRpc {
                        x = gridPosition.x,
                        y = gridPosition.y,
                        playerType = gameClientData.localPlayerType,
                    });
                }

            }
        }
    }

}