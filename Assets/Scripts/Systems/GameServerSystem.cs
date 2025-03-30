using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GameServerSystem : ISystem {


    private const float GRID_SIZE = 3.1f;
    private const int GRID_WIDTH_HEIGHT = 3;


    public struct Line {
        public int2 gridPosition0;
        public int2 gridPosition1;
        public int2 gridPosition2;
        public Orientation orientation;

        public enum Orientation {
            Horizontal,
            Vertical,
            DiagonalA,
            DiagonalB,
        }
    }


    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<GameServerData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        {
            RefRW<GameServerData> gameServerData = SystemAPI.GetSingletonRW<GameServerData>();
            if (gameServerData.ValueRO.state == GameServerData.State.WaitingForPlayers) {
                EntityQuery networkStreamInGameEntityQuery = state.EntityManager.CreateEntityQuery(typeof(NetworkStreamInGame));
                if (networkStreamInGameEntityQuery.CalculateEntityCount() == 2) {
                    // Start Game
                    Debug.Log("Start Game!");
                    gameServerData.ValueRW.state = GameServerData.State.GameStarted;
                    gameServerData.ValueRW.currentPlayablePlayerType = PlayerType.Cross;
                    state.EntityManager.CreateEntity(typeof(GameStartedRpc), typeof(SendRpcCommandRequest));

                    NativeArray<Line> lineArray = new NativeArray<Line>(8, Allocator.Persistent);

                    // Horizontal
                    lineArray[0] = new Line {
                        gridPosition0 = new int2(0, 0),
                        gridPosition1 = new int2(1, 0),
                        gridPosition2 = new int2(2, 0),
                        orientation = Line.Orientation.Horizontal,
                    };
                    lineArray[1] = new Line {
                        gridPosition0 = new int2(0, 1),
                        gridPosition1 = new int2(1, 1),
                        gridPosition2 = new int2(2, 1),
                        orientation = Line.Orientation.Horizontal,
                    };
                    lineArray[2] = new Line {
                        gridPosition0 = new int2(0, 2),
                        gridPosition1 = new int2(1, 2),
                        gridPosition2 = new int2(2, 2),
                        orientation = Line.Orientation.Horizontal,
                    };
                    // Vertical
                    lineArray[3] = new Line {
                        gridPosition0 = new int2(0, 0),
                        gridPosition1 = new int2(0, 1),
                        gridPosition2 = new int2(0, 2),
                        orientation = Line.Orientation.Vertical,
                    };
                    lineArray[4] = new Line {
                        gridPosition0 = new int2(1, 0),
                        gridPosition1 = new int2(1, 1),
                        gridPosition2 = new int2(1, 2),
                        orientation = Line.Orientation.Vertical,
                    };
                    lineArray[5] = new Line {
                        gridPosition0 = new int2(2, 0),
                        gridPosition1 = new int2(2, 1),
                        gridPosition2 = new int2(2, 2),
                        orientation = Line.Orientation.Vertical,
                    };
                    // Diagonal
                    lineArray[6] = new Line {
                        gridPosition0 = new int2(0, 0),
                        gridPosition1 = new int2(1, 1),
                        gridPosition2 = new int2(2, 2),
                        orientation = Line.Orientation.DiagonalA,
                    };
                    lineArray[7] = new Line {
                        gridPosition0 = new int2(0, 2),
                        gridPosition1 = new int2(1, 1),
                        gridPosition2 = new int2(2, 0),
                        orientation = Line.Orientation.DiagonalB,
                    };


                    Entity gameServerDataEntity = SystemAPI.GetSingletonEntity<GameServerData>();
                    state.EntityManager.AddComponentData(gameServerDataEntity, new GameServerDataArrays {
                        playerTypeArray = new NativeArray<PlayerType>(GRID_WIDTH_HEIGHT * GRID_WIDTH_HEIGHT, Allocator.Persistent),
                        lineArray = lineArray,
                    });
                }
            }
        }


        // Handle the Clicked on Grid Position Rpc
        foreach ((
            RefRO<ClickedOnGridPositionRpc> clickedOnGridPositionRpc,
            RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRO<ClickedOnGridPositionRpc>,
                RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess()) {

            Debug.Log("Server, clicked on " + clickedOnGridPositionRpc.ValueRO.x + ", " + clickedOnGridPositionRpc.ValueRO.y + " :: "
                + receiveRpcCommandRequest.ValueRO.SourceConnection + " :: " + clickedOnGridPositionRpc.ValueRO.playerType);

            entityCommandBuffer.DestroyEntity(entity);

            // Check if it's this Player's turn
            RefRW<GameServerData> gameServerData = SystemAPI.GetSingletonRW<GameServerData>();
            if (gameServerData.ValueRO.currentPlayablePlayerType != clickedOnGridPositionRpc.ValueRO.playerType) {
                // Not this Player's turn, skip
                continue;
            }

            // Check if Grid Position is occupied
            RefRW<GameServerDataArrays> gameServerDataArrays = SystemAPI.GetSingletonRW<GameServerDataArrays>();
            if (gameServerDataArrays.ValueRO.playerTypeArray[
                GetFlatIndexFromGridPosition(clickedOnGridPositionRpc.ValueRO.x, clickedOnGridPositionRpc.ValueRO.y)] != PlayerType.None) {
                // Position is already occupied
                continue;
            }

            gameServerDataArrays.ValueRW.playerTypeArray[
                GetFlatIndexFromGridPosition(clickedOnGridPositionRpc.ValueRO.x, clickedOnGridPositionRpc.ValueRO.y)] =
                clickedOnGridPositionRpc.ValueRO.playerType;


            // Swap the current playable player type
            if (gameServerData.ValueRO.currentPlayablePlayerType == PlayerType.Cross) {
                gameServerData.ValueRW.currentPlayablePlayerType = PlayerType.Circle;
            } else {
                gameServerData.ValueRW.currentPlayablePlayerType = PlayerType.Cross;
            }

            Entity playerPrefabEntity;
            switch (clickedOnGridPositionRpc.ValueRO.playerType) {
                default:
                case PlayerType.Cross:
                    playerPrefabEntity = entitiesReferences.crossPrefabEntity;
                    break;
                case PlayerType.Circle:
                    playerPrefabEntity = entitiesReferences.circlePrefabEntity;
                    break;
            }
            Entity playerObjectEntity = entityCommandBuffer.Instantiate(playerPrefabEntity);
            float3 worldPosition = GetWorldPosition(clickedOnGridPositionRpc.ValueRO.x, clickedOnGridPositionRpc.ValueRO.y);
            entityCommandBuffer.SetComponent(playerObjectEntity, LocalTransform.FromPosition(worldPosition));

            Entity clickedOnGridPositionRpcEntity = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(clickedOnGridPositionRpcEntity, new SendRpcCommandRequest());
            entityCommandBuffer.AddComponent(clickedOnGridPositionRpcEntity, clickedOnGridPositionRpc.ValueRO);

            TestWinner(gameServerDataArrays.ValueRO, gameServerData, entityCommandBuffer, entitiesReferences);
        }


        // RematchRpc
        foreach ((
            RefRO<RematchRpc> rematchRpc,
            Entity entity)
            in SystemAPI.Query<RefRO<RematchRpc>>().WithEntityAccess()) {

            Debug.Log("RematchRpc");
            RefRW<GameServerDataArrays> gameServerDataArrays = SystemAPI.GetSingletonRW<GameServerDataArrays>();
            for (int i = 0; i < gameServerDataArrays.ValueRO.playerTypeArray.Length; i++) {
                gameServerDataArrays.ValueRW.playerTypeArray[i] = PlayerType.None;
            }

            RefRW<GameServerData> gameServerData = SystemAPI.GetSingletonRW<GameServerData>();
            gameServerData.ValueRW.currentPlayablePlayerType = PlayerType.Cross;

            foreach ((
                RefRO<SpawnedObject> SpawnedObject,
                Entity spawnedObjectEntity)
                in SystemAPI.Query<
                    RefRO<SpawnedObject>>().WithEntityAccess()) {

                entityCommandBuffer.DestroyEntity(spawnedObjectEntity);
            }

            entityCommandBuffer.DestroyEntity(entity);

            Entity rematchRpcEntity = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(rematchRpcEntity, new RematchRpc());
            entityCommandBuffer.AddComponent(rematchRpcEntity, new SendRpcCommandRequest());
        }

        entityCommandBuffer.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {
        if (SystemAPI.HasSingleton<GameServerDataArrays>()) {
            GameServerDataArrays gameServerDataArrays = SystemAPI.GetSingleton<GameServerDataArrays>();
            gameServerDataArrays.playerTypeArray.Dispose();
            gameServerDataArrays.lineArray.Dispose();
        }
    }

    private float3 GetWorldPosition(int x, int y) {
        return new float3(
            -GRID_SIZE + x * GRID_SIZE,
            -GRID_SIZE + y * GRID_SIZE,
            0
        );
    }

    private int GetFlatIndexFromGridPosition(int x, int y) {
        return
            y * GRID_WIDTH_HEIGHT + 
            x;
    }

    private bool TestWinnerLine(PlayerType aPayerType, PlayerType bPlayerType, PlayerType cPlayerType) {
        return
            aPayerType != PlayerType.None &&
            aPayerType == bPlayerType &&
            bPlayerType == cPlayerType;
    }

    private void TestWinner(GameServerDataArrays gameServerDataArrays, RefRW<GameServerData> gameServerData, EntityCommandBuffer entityCommandBuffer, EntitiesReferences entitiesReferences) {
        foreach (Line line in gameServerDataArrays.lineArray) {
            if (TestWinnerLine(
                gameServerDataArrays.playerTypeArray[GetFlatIndexFromGridPosition(line.gridPosition0.x, line.gridPosition0.y)],
                gameServerDataArrays.playerTypeArray[GetFlatIndexFromGridPosition(line.gridPosition1.x, line.gridPosition1.y)],
                gameServerDataArrays.playerTypeArray[GetFlatIndexFromGridPosition(line.gridPosition2.x, line.gridPosition2.y)]
                )) {

                Debug.Log("Winner!");
                gameServerData.ValueRW.currentPlayablePlayerType = PlayerType.None;

                Entity lineWinnerEntity = entityCommandBuffer.Instantiate(entitiesReferences.lineWinnerPrefabEntity);
                float3 worldPosition = GetWorldPosition(line.gridPosition1.x, line.gridPosition1.y);
                worldPosition.z = -1f;

                float eulerZ = 0f;
                switch (line.orientation) {
                    default:
                    case Line.Orientation.Horizontal: eulerZ =   0f; break;
                    case Line.Orientation.Vertical:   eulerZ =  90f; break;
                    case Line.Orientation.DiagonalA:  eulerZ =  45f; break;
                    case Line.Orientation.DiagonalB:  eulerZ = -45f; break;
                }
                entityCommandBuffer.SetComponent(lineWinnerEntity, new LocalTransform {
                    Position = worldPosition,
                    Rotation = quaternion.RotateZ(eulerZ * math.TORADIANS),
                    Scale = 1f,
                });

                Entity gameWinRpcEntity = entityCommandBuffer.CreateEntity();
                entityCommandBuffer.AddComponent(gameWinRpcEntity, new SendRpcCommandRequest());

                PlayerType winningPlayerType = gameServerDataArrays.playerTypeArray[GetFlatIndexFromGridPosition(line.gridPosition1.x, line.gridPosition1.y)];
                entityCommandBuffer.AddComponent(gameWinRpcEntity, new GameWinRpc {
                    winningPlayerType = winningPlayerType,
                });

                switch (winningPlayerType) {
                    case PlayerType.Cross:
                        gameServerData.ValueRW.playerCrossScore++;
                        break;
                    case PlayerType.Circle:
                        gameServerData.ValueRW.playerCircleScore++;
                        break;
                }

                return;
            }
        }

        bool hasTie = true;
        for (int i = 0; i < gameServerDataArrays.playerTypeArray.Length; i++) {
            if (gameServerDataArrays.playerTypeArray[i] == PlayerType.None) {
                hasTie = false;
                break;
            }
        }
        if (hasTie) {
            Entity gameTieRpcEntity = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(gameTieRpcEntity, new GameTieRpc());
            entityCommandBuffer.AddComponent(gameTieRpcEntity, new SendRpcCommandRequest());
        }
    }

}