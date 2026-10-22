using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Project05.Foundation
{
    /// <summary>
    /// 프리팹별 View GameObject 풀. UnityEngine.Pool.ObjectPool 기반이다 (DESIGN 5.8).
    /// 꺼낸 오브젝트는 활성 씬(Game 씬)에 생기므로 씬과 함께 파괴된다. 그래서 Game 씬 진입·퇴장 시 Clear로 참조를 비운다.
    /// 반납할 때 프리팹을 몰라도 되도록 인스턴스 → 풀 매핑을 함께 관리한다.
    /// </summary>
    public class ViewPool
    {
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _poolsByPrefab = new Dictionary<GameObject, ObjectPool<GameObject>>();
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _poolsByInstance = new Dictionary<GameObject, ObjectPool<GameObject>>();

        private const int DEFAULT_POOL_CAPACITY = 32;
        private const int MAX_POOL_SIZE = 512;

        /// <summary>
        /// 프리팹의 인스턴스를 풀에서 꺼내 활성화한다. 풀이 비었으면 새로 만든다.
        /// </summary>
        /// <param name="prefab">꺼낼 프리팹 (null이면 null 반환)</param>
        /// <returns>활성화된 인스턴스</returns>
        public GameObject Get(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            ObjectPool<GameObject> pool;
            if (!_poolsByPrefab.TryGetValue(prefab, out pool))
            {
                pool = CreatePool(prefab);
                _poolsByPrefab.Add(prefab, pool);
            }

            GameObject instance = pool.Get();
            _poolsByInstance[instance] = pool;
            return instance;
        }

        /// <summary>
        /// 꺼냈던 인스턴스를 비활성화해 풀에 돌려준다. 이 풀에서 꺼낸 것이 아니거나 이미 파괴됐으면 무시한다.
        /// </summary>
        /// <param name="instance">돌려줄 인스턴스</param>
        public void Release(GameObject instance)
        {
            ObjectPool<GameObject> pool;
            if (instance == null || !_poolsByInstance.TryGetValue(instance, out pool))
            {
                return;
            }

            _poolsByInstance.Remove(instance);
            pool.Release(instance);
        }

        /// <summary>
        /// 모든 풀과 매핑을 비운다. Game 씬 진입·퇴장 시 Runner가 호출한다.
        /// 씬과 함께 이미 파괴된 오브젝트는 건너뛰고, 남아 있는 오브젝트만 파괴한다.
        /// </summary>
        public void Clear()
        {
            foreach (KeyValuePair<GameObject, ObjectPool<GameObject>> poolEntry in _poolsByPrefab)
            {
                poolEntry.Value.Clear();
            }
            foreach (KeyValuePair<GameObject, ObjectPool<GameObject>> instanceEntry in _poolsByInstance)
            {
                DestroyIfAlive(instanceEntry.Key);
            }
            _poolsByPrefab.Clear();
            _poolsByInstance.Clear();
        }

        private static ObjectPool<GameObject> CreatePool(GameObject prefab)
        {
            return new ObjectPool<GameObject>(
                () => Object.Instantiate(prefab),
                ActivateInstance,
                DeactivateInstance,
                DestroyIfAlive,
                false,
                DEFAULT_POOL_CAPACITY,
                MAX_POOL_SIZE);
        }

        private static void ActivateInstance(GameObject instance)
        {
            instance.SetActive(true);
        }

        private static void DeactivateInstance(GameObject instance)
        {
            instance.SetActive(false);
        }

        private static void DestroyIfAlive(GameObject instance)
        {
            // 씬 언로드로 파괴된 오브젝트는 Unity의 null 비교에서 null이다
            if (instance != null)
            {
                Object.Destroy(instance);
            }
        }
    }
}
