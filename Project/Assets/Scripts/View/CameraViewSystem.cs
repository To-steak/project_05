using Project05.Component;
using Unity.Cinemachine;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// Cinemachine 추적 대상(히어로 View Transform) 연결, 탑승 시 카메라 전환. 히어로 보간 이후에 호출되어야 한다.
    /// Runner가 LateUpdate에서 정해진 순서로 호출한다. World는 읽기만 한다.
    /// 구현 브랜치: feat/hero-movement (추적), feat/camera-rotate (Q/E 회전), feat/tower (탑승 시 전환)
    /// </summary>
    public static class CameraViewSystem
    {
        /// <summary>
        /// 카메라 추적 대상과 전환 상태를 갱신한다.
        /// 추적 대상은 Logic 위치가 아니라 보간된 히어로 View Transform이다. 데드존·감쇠는 씬의 CinemachinePositionComposer 설정을 따른다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
            HeroCameraView heroCameraView = viewContext.heroCameraView;
            HeroView heroView = viewContext.heroView;
            if (heroCameraView == null || heroView == null)
            {
                return;
            }

            CinemachineCamera heroCamera = heroCameraView.CinemachineCamera;
            if (heroCamera == null)
            {
                return;
            }

            Transform heroTransform = heroView.transform;
            if (heroCamera.Follow != heroTransform)
            {
                heroCamera.Follow = heroTransform;
                // 대상을 처음 잡을 때는 감쇠 없이 바로 맞춘다 (원점에서 쓸려 오는 것을 막음)
                heroCamera.PreviousStateIsValid = false;
            }

            RotateAroundHero(world, viewContext, heroCameraView);
        }

        /// <summary>
        /// Q/E를 누르고 있는 동안 Follow 오프셋을 히어로 기준 Y축으로 돌린다. 높이·거리는 씬에서 정한 기준 오프셋 그대로다.
        /// 게임 중(일시정지·패배 아님)에만 돈다. 감쇠는 씬의 Position Damping을 따른다.
        /// 카메라는 View라 World에 영향이 없다. 클릭 이동·화면 밖 화살표는 Camera.main을 쓰므로 회전을 그대로 따른다.
        /// </summary>
        private static void RotateAroundHero(World world, ViewContext viewContext, HeroCameraView heroCameraView)
        {
            CinemachineFollow follow = heroCameraView.CinemachineFollow;
            if (follow == null)
            {
                return;
            }
            if (!viewContext.hasCameraBaseFollowOffset)
            {
                viewContext.cameraBaseFollowOffset = follow.FollowOffset;
                viewContext.hasCameraBaseFollowOffset = true;
            }

            bool isPlaying = world.pauseReason == PauseReason.None && world.flow.phase != GamePhase.Defeat;
            if (!isPlaying || viewContext.cameraRotateInput == 0f)
            {
                return;
            }

            viewContext.cameraYawDegrees += viewContext.cameraRotateInput * heroCameraView.RotationDegreesPerSecond * Time.deltaTime;
            follow.FollowOffset = Quaternion.Euler(0f, viewContext.cameraYawDegrees, 0f) * viewContext.cameraBaseFollowOffset;
        }
    }
}
