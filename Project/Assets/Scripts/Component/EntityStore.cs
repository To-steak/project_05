using System;

namespace Project05.Component
{
    /// <summary>
    /// 고정 용량 엔티티 저장소. 배열 + 빈 슬롯 스택 + 슬롯별 generation으로 관리한다.
    /// 생성 후에는 할당이 없다. 순회는 0 ~ Capacity - 1 슬롯을 돌며 IsAliveAt으로 거른다.
    /// </summary>
    /// <typeparam name="T">엔티티 struct</typeparam>
    public class EntityStore<T> where T : struct
    {
        private readonly T[] _items;
        private readonly int[] _generations;
        private readonly bool[] _isAliveFlags;
        private readonly int[] _freeIndices;
        private int _freeCount;
        private int _aliveCount;

        /// <summary>
        /// 저장소 최대 용량
        /// </summary>
        public int Capacity
        {
            get { return _items.Length; }
        }

        /// <summary>
        /// 살아있는 엔티티 수
        /// </summary>
        public int Count
        {
            get { return _aliveCount; }
        }

        /// <summary>
        /// 용량만큼 슬롯을 미리 만든다.
        /// </summary>
        /// <param name="capacity">최대 엔티티 수 (0 이상)</param>
        public EntityStore(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _items = new T[capacity];
            _generations = new int[capacity];
            _isAliveFlags = new bool[capacity];
            _freeIndices = new int[capacity];

            // 0번 슬롯부터 쓰이도록 역순으로 쌓는다
            for (int index = 0; index < capacity; index++)
            {
                _freeIndices[index] = capacity - 1 - index;
            }
            _freeCount = capacity;
            _aliveCount = 0;
        }

        /// <summary>
        /// 빈 슬롯에 새 엔티티를 만든다. 슬롯 값은 default로 초기화되고 generation이 1 오른다.
        /// </summary>
        /// <param name="entityID">만들어진 엔티티의 ID. 실패하면 무효 ID</param>
        /// <returns>용량이 남아 생성했으면 true, 가득 찼으면 false</returns>
        public bool TryCreate(out EntityID entityID)
        {
            if (_freeCount == 0)
            {
                entityID = default(EntityID);
                return false;
            }

            _freeCount--;
            int index = _freeIndices[_freeCount];
            _generations[index]++;
            _isAliveFlags[index] = true;
            _items[index] = default(T);
            _aliveCount++;

            entityID = new EntityID(index, _generations[index]);
            return true;
        }

        /// <summary>
        /// ID가 가리키는 엔티티가 아직 살아있는지 확인한다. 슬롯이 재사용됐으면 false.
        /// </summary>
        /// <param name="entityID">확인할 ID</param>
        /// <returns>살아있으면 true</returns>
        public bool IsAlive(EntityID entityID)
        {
            int index = entityID.index;
            return index >= 0
                && index < _items.Length
                && _isAliveFlags[index]
                && _generations[index] == entityID.generation;
        }

        /// <summary>
        /// 살아있는 엔티티를 참조로 가져온다. 죽은 ID면 예외를 던진다.
        /// </summary>
        /// <param name="entityID">가져올 엔티티 ID</param>
        /// <returns>엔티티 struct 참조</returns>
        public ref T Get(EntityID entityID)
        {
            if (!IsAlive(entityID))
            {
                throw new InvalidOperationException("EntityStore.Get: 살아있지 않은 엔티티 " + entityID);
            }
            return ref _items[entityID.index];
        }

        /// <summary>
        /// 엔티티를 즉시 소멸시킨다. 이미 죽은 ID면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="entityID">소멸시킬 엔티티 ID</param>
        /// <returns>소멸시켰으면 true, 이미 죽어 있었으면 false</returns>
        public bool Destroy(EntityID entityID)
        {
            if (!IsAlive(entityID))
            {
                return false;
            }

            int index = entityID.index;
            _isAliveFlags[index] = false;
            _freeIndices[_freeCount] = index;
            _freeCount++;
            _aliveCount--;
            return true;
        }

        /// <summary>
        /// 순회용. 해당 슬롯에 살아있는 엔티티가 있는지 확인한다.
        /// </summary>
        /// <param name="index">슬롯 번호 (0 ~ Capacity - 1)</param>
        /// <returns>살아있으면 true</returns>
        public bool IsAliveAt(int index)
        {
            return _isAliveFlags[index];
        }

        /// <summary>
        /// 순회용. 슬롯의 엔티티를 참조로 가져온다. IsAliveAt으로 먼저 확인한다.
        /// </summary>
        /// <param name="index">슬롯 번호 (0 ~ Capacity - 1)</param>
        /// <returns>엔티티 struct 참조</returns>
        public ref T GetAt(int index)
        {
            return ref _items[index];
        }

        /// <summary>
        /// 순회용. 슬롯에 들어 있는 엔티티의 현재 ID를 만든다.
        /// </summary>
        /// <param name="index">슬롯 번호 (0 ~ Capacity - 1)</param>
        /// <returns>슬롯의 현재 세대 ID</returns>
        public EntityID GetIDAt(int index)
        {
            return new EntityID(index, _generations[index]);
        }
    }
}
