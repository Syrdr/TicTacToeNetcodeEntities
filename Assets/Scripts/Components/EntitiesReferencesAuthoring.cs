using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour {


    public GameObject crossPrefabGameObject;
    public GameObject circlePrefabGameObject;
    public GameObject lineWinnerPrefabGameObject;
    public GameObject placeSfxPrefabGameObject;
    public GameObject winSfxPrefabGameObject;
    public GameObject loseSfxPrefabGameObject;


    public class Baker : Baker<EntitiesReferencesAuthoring> {

        public override void Bake(EntitiesReferencesAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences {
                crossPrefabEntity = GetEntity(authoring.crossPrefabGameObject, TransformUsageFlags.Dynamic),
                circlePrefabEntity = GetEntity(authoring.circlePrefabGameObject, TransformUsageFlags.Dynamic),
                lineWinnerPrefabEntity = GetEntity(authoring.lineWinnerPrefabGameObject, TransformUsageFlags.Dynamic),
                placeSfxPrefabEntity = GetEntity(authoring.placeSfxPrefabGameObject, TransformUsageFlags.Dynamic),
                winSfxPrefabEntity = GetEntity(authoring.winSfxPrefabGameObject, TransformUsageFlags.Dynamic),
                loseSfxPrefabEntity = GetEntity(authoring.loseSfxPrefabGameObject, TransformUsageFlags.Dynamic),
            });
        }
    }
}


public struct EntitiesReferences : IComponentData {

    public Entity crossPrefabEntity;
    public Entity circlePrefabEntity;
    public Entity lineWinnerPrefabEntity;
    public Entity placeSfxPrefabEntity;
    public Entity winSfxPrefabEntity;
    public Entity loseSfxPrefabEntity;

}