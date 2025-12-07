using System.Collections.Generic;

namespace Pools {
    public class GemPool {
        private const int POOL_SIZE = 50;
        private const int POOL_GROW_AMOUNT = 10;

        private readonly Dictionary<GemColor, Dictionary<GemType, MonoBehaviourObjectPool<GemView>>> _gemPools;

        private readonly Dictionary<GemColor, Dictionary<GemType, MonoBehaviourObjectPool<GemDestroyEffect>>>
            _destroyEffectPools;

        private readonly GemRepository gemRepository;

        public GemPool(GemRepository gemRepository) {
            this.gemRepository = gemRepository;
            //no pool for GemType.None 
            _gemPools = new Dictionary<GemColor, Dictionary<GemType, MonoBehaviourObjectPool<GemView>>>();
            _destroyEffectPools =
                new Dictionary<GemColor, Dictionary<GemType, MonoBehaviourObjectPool<GemDestroyEffect>>>();

            Initialize();
        }

        public GemView GetGem(Gem gem) {
            if (gem == null || !_gemPools.TryGetValue(gem.Color, out var poolByType) || !poolByType.TryGetValue(gem.Type, out var pool)) {
                return null;
            }

            return pool.Get();
        }

        public void ReleaseGem(GemView gemView) {
            if (!gemView || !_gemPools.TryGetValue(gemView.Gem.Color, out var poolByType) ||
                !poolByType.TryGetValue(gemView.Gem.Type, out var pool)) {
                return;
            }

            pool.Return(gemView);
        }

        public GemDestroyEffect GetDestroyEffect(Gem gem) {
            if (gem == null || !_destroyEffectPools.TryGetValue(gem.Color, out var poolByType) ||
                !poolByType.TryGetValue(gem.Type, out var pool)) {
                return null;
            }

            return pool.Get();
        }

        public void ReleaseDestroyEffect(Gem gem, GemDestroyEffect effect) {
            if (gem == null || !effect || !_destroyEffectPools.TryGetValue(gem.Color, out var poolByType) ||
                !poolByType.TryGetValue(gem.Type, out var pool)) {
                return;
            }

            pool.Return(effect);
        }

        private void Initialize() {
            foreach (var gemPrefabConfig in gemRepository.AllGems()) {
                var gemPool = new MonoBehaviourObjectPool<GemView>(gemPrefabConfig.GemPrefab, POOL_SIZE, POOL_GROW_AMOUNT);
                gemPool.WarmUp();
                if (_gemPools.TryGetValue(gemPrefabConfig.Color, out var poolByType)) {
                    poolByType[gemPrefabConfig.Type] = gemPool;
                } else {
                    _gemPools.Add(gemPrefabConfig.Color,
                        new Dictionary<GemType, MonoBehaviourObjectPool<GemView>> { { gemPrefabConfig.Type, gemPool } });
                }

                var destroyEffectPool =
                    new MonoBehaviourObjectPool<GemDestroyEffect>(gemPrefabConfig.DestroyEffect, POOL_SIZE,
                        POOL_GROW_AMOUNT);
                destroyEffectPool.WarmUp();
                if (_destroyEffectPools.TryGetValue(gemPrefabConfig.Color, out var destroyEffectPoolByType)) {
                    destroyEffectPoolByType[gemPrefabConfig.Type] = destroyEffectPool;
                } else {
                    _destroyEffectPools.Add(gemPrefabConfig.Color,
                        new Dictionary<GemType, MonoBehaviourObjectPool<GemDestroyEffect>>
                            { { gemPrefabConfig.Type, destroyEffectPool } });
                }
            }
        }
    }
}