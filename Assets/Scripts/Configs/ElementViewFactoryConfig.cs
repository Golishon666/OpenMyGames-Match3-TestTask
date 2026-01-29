using UnityEngine;

[CreateAssetMenu(fileName = "ElementViewFactoryConfig", menuName = "GameConfigs/ElementViewFactoryConfig")]
public class ElementViewFactoryConfig : ScriptableObject
{
    [System.Serializable]
    public struct ElementPrefabPair
    {
        public ElementType type;
        public ElementView prefab;
    }

    public ElementPrefabPair[] elementPrefabs;
}