using Unity.Cinemachine;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 히어로를 따라가는 Cinemachine 카메라 표시. Game 씬의 CinemachineCamera 오브젝트에 붙인다.
    /// 탑승용 카메라가 추가되어도 히어로 카메라를 구분할 수 있게 한다. 로직 없음.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class HeroCameraView : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _cinemachineCamera;

        /// <summary>
        /// 같은 오브젝트의 Cinemachine 카메라.
        /// </summary>
        public CinemachineCamera CinemachineCamera
        {
            get { return _cinemachineCamera; }
        }

        private void Reset()
        {
            _cinemachineCamera = GetComponent<CinemachineCamera>();
        }

        private void Awake()
        {
            if (_cinemachineCamera == null)
            {
                _cinemachineCamera = GetComponent<CinemachineCamera>();
            }
        }
    }
}
