using System;

namespace Project05.Component
{
    /// <summary>
    /// 엔티티 식별자. 저장소 슬롯 index와 슬롯 재사용 세대 generation으로 엔티티를 가리킨다.
    /// generation이 0이면 무효 ID다 (저장소는 generation을 1부터 쓴다).
    /// </summary>
    public struct EntityID : IEquatable<EntityID>
    {
        // 저장소 배열의 슬롯 번호
        public int index;
        // 슬롯 재사용 세대. 0이면 무효
        public int generation;

        /// <summary>
        /// 무효 ID가 아닌지 확인한다. 생존 여부는 저장소의 IsAlive로 확인해야 한다.
        /// </summary>
        public bool IsValid
        {
            get { return generation != 0; }
        }

        /// <summary>
        /// 슬롯 번호와 세대로 ID를 만든다.
        /// </summary>
        /// <param name="index">저장소 슬롯 번호</param>
        /// <param name="generation">슬롯 재사용 세대</param>
        public EntityID(int index, int generation)
        {
            this.index = index;
            this.generation = generation;
        }

        /// <summary>
        /// 두 ID가 같은 슬롯, 같은 세대인지 비교한다. Dictionary 키로 쓸 때 박싱을 막는다.
        /// </summary>
        /// <param name="other">비교할 ID</param>
        /// <returns>같으면 true</returns>
        public bool Equals(EntityID other)
        {
            return index == other.index && generation == other.generation;
        }

        /// <summary>
        /// object 비교. 박싱이 생기므로 가능하면 Equals(EntityID)나 == 를 쓴다.
        /// </summary>
        /// <param name="other">비교할 객체</param>
        /// <returns>같은 ID면 true</returns>
        public override bool Equals(object other)
        {
            return other is EntityID otherID && Equals(otherID);
        }

        /// <summary>
        /// 해시 값을 만든다.
        /// </summary>
        /// <returns>index와 generation을 섞은 해시</returns>
        public override int GetHashCode()
        {
            return (index * 397) ^ generation;
        }

        /// <summary>
        /// 디버그 표시용 문자열을 만든다.
        /// </summary>
        /// <returns>"index:generation" 형식 문자열</returns>
        public override string ToString()
        {
            return index + ":" + generation;
        }

        /// <summary>
        /// 두 ID가 같은지 비교한다.
        /// </summary>
        /// <param name="left">왼쪽 ID</param>
        /// <param name="right">오른쪽 ID</param>
        /// <returns>같으면 true</returns>
        public static bool operator ==(EntityID left, EntityID right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 두 ID가 다른지 비교한다.
        /// </summary>
        /// <param name="left">왼쪽 ID</param>
        /// <param name="right">오른쪽 ID</param>
        /// <returns>다르면 true</returns>
        public static bool operator !=(EntityID left, EntityID right)
        {
            return !left.Equals(right);
        }
    }
}
