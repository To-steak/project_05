using System.Collections.Generic;
using Project05.Foundation;
using Project05.Component;
using UnityEngine;

namespace Project05.View
{
    /// <summary>
    /// 한 번 재생하는 효과와 떠오르는 문구의 공용 처리. 꺼내기(Spawn~)는 각 ViewSystem이 자기 기록을 읽고 호출하고,
    /// Present가 끝난 것을 App.Pool에 돌려주고 문구를 떠올린다. 상태는 ViewContext 목록에만 둔다.
    /// 효과는 루트 파티클이 끝나면(IsAlive false) 돌려준다. 길이는 파티클 설정이 정한다.
    /// 문구는 프리팹의 유지 틱을 Time.time으로 바꿔 잰다.
    /// 월드가 멈춘 동안(일시정지·패배, ViewInterpolation.IsSimulationStopped) 효과 파티클과 문구도 그 자리에 멈춘다.
    /// Runner가 LateUpdate에서 효과를 꺼내는 ViewSystem들 다음에 호출한다. World는 읽기만 한다.
    /// 구현 브랜치: feat/vfx
    /// </summary>
    public static class VisualEffectViewSystem
    {
        /// <summary>
        /// 재생이 끝난 효과와 유지 시간이 지난 문구를 풀에 돌려주고, 남은 문구를 떠올리며 카메라를 향하게 한다.
        /// </summary>
        /// <param name="world">현재 판의 월드 (읽기 전용)</param>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        public static void Present(World world, ViewContext viewContext)
        {
            bool isStopped = ViewInterpolation.IsSimulationStopped(world);
            SynchronizePause(viewContext, isStopped);
            if (isStopped)
            {
                // 멈춘 동안은 반납도 진행도 하지 않는다. 문구는 흐른 시간만큼 시작·끝 시각을 미뤄 그 자리에 둔다
                HoldFloatingTexts(viewContext, Time.deltaTime);
                return;
            }

            ReleaseEndedEffects(viewContext);
            PresentFloatingTexts(viewContext, Time.time);
        }

        /// <summary>
        /// 종류의 효과를 위치에 한 번 재생한다. 프리팹이 비어 있으면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        /// <param name="effectKind">효과 종류</param>
        /// <param name="position">효과 원점 (바닥 기준 엔티티 위치)</param>
        /// <param name="scale">프리팹 크기에 곱할 배율 (근거리 범위는 반지름)</param>
        public static void Spawn(ViewContext viewContext, VisualEffectKind effectKind, Vector3 position, float scale)
        {
            SpawnFacing(viewContext, effectKind, position, Quaternion.identity, scale);
        }

        /// <summary>
        /// 종류의 효과를 위치에 방향을 주어 한 번 재생한다 (근거리 부채꼴처럼 방향이 있는 효과). 프리팹 회전 위에 방향을 곱한다.
        /// 프리팹이 비어 있으면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        /// <param name="effectKind">효과 종류</param>
        /// <param name="position">효과 원점 (바닥 기준 엔티티 위치)</param>
        /// <param name="facing">효과가 향할 방향 (프리팹의 +Z가 이 방향을 향한다)</param>
        /// <param name="scale">프리팹 크기에 곱할 배율 (근거리 범위는 반지름)</param>
        public static void SpawnFacing(ViewContext viewContext, VisualEffectKind effectKind, Vector3 position, Quaternion facing, float scale)
        {
            GameObject[] effectPrefabs = App.ViewData.visualEffectPrefabs;
            int kindIndex = (int)effectKind;
            if (kindIndex < 0 || kindIndex >= effectPrefabs.Length)
            {
                return;
            }
            SpawnPrefab(viewContext, effectPrefabs[kindIndex], position, facing, scale);
        }

        /// <summary>
        /// 효과 프리팹을 풀에서 꺼내 위치에 한 번 재생한다. 종류가 엔티티마다 다른 효과(적 사망)에 쓴다.
        /// </summary>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        /// <param name="prefab">효과 프리팹 (null이면 아무것도 하지 않는다)</param>
        /// <param name="position">효과 원점 (바닥 기준 엔티티 위치)</param>
        /// <param name="scale">프리팹 크기에 곱할 배율</param>
        public static void SpawnPrefab(ViewContext viewContext, GameObject prefab, Vector3 position, float scale)
        {
            SpawnPrefab(viewContext, prefab, position, Quaternion.identity, scale);
        }

        private static void SpawnPrefab(ViewContext viewContext, GameObject prefab, Vector3 position, Quaternion facing, float scale)
        {
            if (prefab == null)
            {
                return;
            }

            GameObject instance = App.Pool.Get(prefab);
            VisualEffectView effectView = instance.GetComponent<VisualEffectView>();
            if (effectView == null)
            {
                Debug.LogError("[VisualEffectViewSystem] 효과 프리팹 " + prefab.name + "에 VisualEffectView 컴포넌트가 없다.", prefab);
                App.Pool.Release(instance);
                return;
            }
            if (!effectView.HasParticle)
            {
                Debug.LogError("[VisualEffectViewSystem] 효과 프리팹 " + prefab.name + "의 루트에 ParticleSystem이 없다. 끝을 알 수 없어 재생하지 않는다.", prefab);
                App.Pool.Release(instance);
                return;
            }

            Transform effectTransform = instance.transform;
            effectTransform.SetPositionAndRotation(position, facing * prefab.transform.rotation);
            effectTransform.localScale = prefab.transform.localScale * scale;
            effectView.Restart();

            ActiveVisualEffect activeEffect = new ActiveVisualEffect();
            activeEffect.effectView = effectView;
            viewContext.activeVisualEffects.Add(activeEffect);
        }

        /// <summary>
        /// 떠오르는 문구를 위치에 띄운다. 프리팹이 비어 있으면 아무것도 하지 않는다.
        /// </summary>
        /// <param name="viewContext">현재 판의 화면 상태</param>
        /// <param name="prefab">문구 프리팹 (null이면 아무것도 하지 않는다)</param>
        /// <param name="position">문구가 나올 위치</param>
        /// <param name="text">표시할 문구</param>
        public static void SpawnFloatingText(ViewContext viewContext, GameObject prefab, Vector3 position, string text)
        {
            if (prefab == null)
            {
                return;
            }

            GameObject instance = App.Pool.Get(prefab);
            FloatingTextView textView = instance.GetComponent<FloatingTextView>();
            if (textView == null)
            {
                Debug.LogError("[VisualEffectViewSystem] 문구 프리팹 " + prefab.name + "에 FloatingTextView 컴포넌트가 없다.", prefab);
                App.Pool.Release(instance);
                return;
            }

            textView.SetText(text);
            textView.SetProgress(0f);
            textView.transform.position = position;

            ActiveFloatingText activeText = new ActiveFloatingText();
            activeText.textView = textView;
            activeText.startPosition = position;
            activeText.startTime = Time.time;
            activeText.endTime = activeText.startTime + textView.LifetimeTicks * Time.fixedDeltaTime;
            viewContext.activeFloatingTexts.Add(activeText);
        }

        /// <summary>
        /// 월드가 멈추거나 다시 움직인 순간에만 재생 중인 효과를 모두 멈추거나 이어 재생한다.
        /// 멈춘 동안에는 새 기록이 없어 새 효과가 나오지 않으므로 목록만 처리하면 된다.
        /// </summary>
        private static void SynchronizePause(ViewContext viewContext, bool isStopped)
        {
            if (viewContext.areVisualEffectsPaused == isStopped)
            {
                return;
            }
            viewContext.areVisualEffectsPaused = isStopped;

            List<ActiveVisualEffect> activeEffects = viewContext.activeVisualEffects;
            for (int effectIndex = 0; effectIndex < activeEffects.Count; effectIndex++)
            {
                VisualEffectView effectView = activeEffects[effectIndex].effectView;
                if (effectView == null)
                {
                    continue;
                }
                if (isStopped)
                {
                    effectView.Pause();
                }
                else
                {
                    effectView.Resume();
                }
            }
        }

        /// <summary>
        /// 멈춘 프레임의 시간만큼 문구의 시작·끝 시각을 미룬다. 다시 움직이면 멈춘 자리부터 이어서 떠오른다.
        /// </summary>
        private static void HoldFloatingTexts(ViewContext viewContext, float deltaTime)
        {
            List<ActiveFloatingText> activeTexts = viewContext.activeFloatingTexts;
            for (int textIndex = 0; textIndex < activeTexts.Count; textIndex++)
            {
                ActiveFloatingText activeText = activeTexts[textIndex];
                activeText.startTime += deltaTime;
                activeText.endTime += deltaTime;
                activeTexts[textIndex] = activeText;
            }
        }

        /// <summary>
        /// 루트 파티클이 끝난 효과를 풀에 돌려준다. 뒤에서부터 돌며 마지막 원소와 바꿔 지운다 (순서는 상관없다).
        /// </summary>
        private static void ReleaseEndedEffects(ViewContext viewContext)
        {
            List<ActiveVisualEffect> activeEffects = viewContext.activeVisualEffects;
            for (int effectIndex = activeEffects.Count - 1; effectIndex >= 0; effectIndex--)
            {
                VisualEffectView effectView = activeEffects[effectIndex].effectView;
                if (effectView != null && effectView.IsAlive)
                {
                    continue;
                }

                if (effectView != null)
                {
                    App.Pool.Release(effectView.gameObject);
                }
                RemoveAtBySwap(activeEffects, effectIndex);
            }
        }

        /// <summary>
        /// 문구를 진행도만큼 떠올리고 흐리게 하며 카메라와 같은 방향을 보게 한다. 끝난 문구는 풀에 돌려준다.
        /// </summary>
        private static void PresentFloatingTexts(ViewContext viewContext, float now)
        {
            List<ActiveFloatingText> activeTexts = viewContext.activeFloatingTexts;
            Camera mainCamera = viewContext.mainCamera;
            for (int textIndex = activeTexts.Count - 1; textIndex >= 0; textIndex--)
            {
                ActiveFloatingText activeText = activeTexts[textIndex];
                FloatingTextView textView = activeText.textView;
                if (textView == null || now >= activeText.endTime)
                {
                    if (textView != null)
                    {
                        App.Pool.Release(textView.gameObject);
                    }
                    RemoveAtBySwap(activeTexts, textIndex);
                    continue;
                }

                float progress = Mathf.InverseLerp(activeText.startTime, activeText.endTime, now);
                Vector3 position = activeText.startPosition + Vector3.up * (textView.RiseDistance * progress);
                if (mainCamera != null)
                {
                    textView.transform.SetPositionAndRotation(position, mainCamera.transform.rotation);
                }
                else
                {
                    textView.transform.position = position;
                }
                textView.SetProgress(progress);
            }
        }

        private static void RemoveAtBySwap<T>(List<T> list, int index)
        {
            int lastIndex = list.Count - 1;
            list[index] = list[lastIndex];
            list.RemoveAt(lastIndex);
        }
    }
}
