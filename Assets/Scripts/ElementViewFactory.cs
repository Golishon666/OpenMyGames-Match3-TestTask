using System.Collections.Generic;

    public class ElementViewFactory
    {
        private readonly Dictionary<ElementType, ElementView> _prefabMap;

        public ElementViewFactory(ElementViewFactoryConfig config)
        {
            _prefabMap = new Dictionary<ElementType, ElementView>();
            foreach (var pair in config.elementPrefabs)
            {
                if (!_prefabMap.ContainsKey(pair.type))
                    _prefabMap.Add(pair.type, pair.prefab);
            }
        }

        public ElementView GetPrefab(ElementType type)
        {
            return _prefabMap.TryGetValue(type, out var prefab)
                ? prefab
                : null;
        }
    }
