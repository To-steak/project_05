using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 히어로를 따라가는 Cinemachine 카메라 표시. Game 씬의 CinemachineCamera 오브젝트에 붙인다.
    /// 탑승용 카메라가 추가되어도 히어로 카메라를 구분할 수 있게 한다. 로직 없음.
    /// Q/E 회전: 같은 오브젝트의 CinemachineFollow 오프셋을 히어로 기준 Y축으로 돌린다 (CameraViewSystem).
    /// 회전한 오프셋이 그대로 적용되도록 판 시작 시 Binding Mode를 World Space로 바꾼다.
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class HeroCameraView : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _cinemachineCamera;
        [SerializeField] private CinemachineFollow _cinemachineFollow;
        // Q/E를 누르고 있을 때 초당 회전 각도
        [SerializeField, Min(0f)] private float _rotationDegreesPerSecond = 90f;

        /// <summary>
        /// 같은 오브젝트의 Cinemachine 카메라.
        /// </summary>
        public CinemachineCamera CinemachineCamera
        {
            get { return _cinemachineCamera; }
        }

        /// <summary>
        /// 같은 오브젝트의 Cinemachine Follow (없으면 null, 그때는 회전하지 않는다).
        /// </summary>
        public CinemachineFollow CinemachineFollow
        {
            get { return _cinemachineFollow; }
        }

        /// <summary>
        /// Q/E를 누르고 있을 때 초당 회전 각도
        /// </summary>
        public float RotationDegreesPerSecond
        {
            get { return _rotationDegreesPerSecond; }
        }

        private void Reset()
        {
            _cinemachineCamera = GetComponent<CinemachineCamera>();
            _cinemachineFollow = GetComponent<CinemachineFollow>();
        }

        private void Awake()
        {
            if (_cinemachineCamera == null)
            {
                _cinemachineCamera = GetComponent<CinemachineCamera>();
            }
            if (_cinemachineFollow == null)
            {
                _cinemachineFollow = GetComponent<CinemachineFollow>();
            }
            if (_cinemachineFollow != null)
            {
                // Lazy Follow는 오프셋을 카메라 기준으로 해석해 회전한 오프셋이 그대로 적용되지 않는다
                _cinemachineFollow.TrackerSettings.BindingMode = BindingMode.WorldSpace;
            }
        }
    }
}
