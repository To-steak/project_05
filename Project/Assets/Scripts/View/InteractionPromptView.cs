using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// Space 아이콘 오브젝트. Game 씬에 하나 두면 판 시작 시 Runner가 찾는다. 로직 없음.
    /// 대상 위치에서 _heightOffset만큼 위에 띄우고, 켜면 카메라를 바라보게 한다.
    /// </summary>
    public class InteractionPromptView : MonoBehaviour
    {
        // 대상 위치에서 아이콘을 띄울 높이 (월드 유닛)
        [SerializeField] private float _heightOffset = 2f;
        [SerializeField] private bool _isFacingCamera = true;

        /// <summary>
        /// 대상 위치에서 아이콘을 띄울 높이 (월드 유닛)
        /// </summary>
        public float HeightOffset
        {
            get { return _heightOffset; }
        }

        /// <summary>
        /// 카메라를 바라보게 돌릴지 (빌보드)
        /// </summary>
        public bool IsFacingCamera
        {
            get { return _isFacingCamera; }
        }

        /// <summary>
        /// 아이콘을 보이거나 숨긴다.
        /// </summary>
        /// <param name="isVisible">보이면 true</param>
        public void SetVisible(bool isVisible)
        {
            if (gameObject.activeSelf != isVisible)
            {
                gameObject.SetActive(isVisible);
            }
        }
    }
}
