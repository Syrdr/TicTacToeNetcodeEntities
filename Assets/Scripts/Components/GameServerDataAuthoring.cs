using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class GameServerDataAuthoring : MonoBehaviour {

    public class Baker : Baker<GameServerDataAuthoring> {

        public override void Bake(GameServerDataAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new GameServerData { });
        }
    }
}



public struct GameServerData : IComponentData {

    public enum State {
        WaitingForPlayers,
        GameStarted
    }

    public State state;

    [GhostField] public PlayerType currentPlayablePlayerType;
    [GhostField] public int playerCrossScore;
    [GhostField] public int playerCircleScore;

}

public struct GameServerDataArrays : IComponentData {

    public NativeArray<PlayerType> playerTypeArray;
    public NativeArray<GameServerSystem.Line> lineArray;

}