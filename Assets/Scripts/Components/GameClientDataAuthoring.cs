using Unity.Entities;
using UnityEngine;

public class GameClientDataAuthoring : MonoBehaviour {


    public class Baker : Baker<GameClientDataAuthoring> {

        public override void Bake(GameClientDataAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new GameClientData {
            });
        }
    }
}



public struct GameClientData : IComponentData {

    public PlayerType localPlayerType;

}