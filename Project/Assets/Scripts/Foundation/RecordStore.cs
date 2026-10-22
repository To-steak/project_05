using UnityEngine;

namespace Project05.Foundation
{
    /// <summary>
    /// 최고 점수와 최고 도달 웨이브를 PlayerPrefs에 저장·로드한다 (DESIGN 3.4.2).
    /// 도달 웨이브는 누적 웨이브 번호 w = (n - 1) × 10 + x 하나로 저장·비교하고, 표시할 때만 n-x로 바꾼다.
    /// 패배를 Core(Runner)가 감지했을 때 TryUpdate로 갱신한다. System은 저장에 관여하지 않는다.
    /// </summary>
    public class RecordStore
    {
        private int _bestScore;
        private int _bestCumulativeWaveNumber;

        private const string BEST_SCORE_KEY = "Records.BestScore";
        private const string BEST_CUMULATIVE_WAVE_KEY = "Records.BestCumulativeWave";

        /// <summary>
        /// 최고 점수 (기록 없으면 0)
        /// </summary>
        public int BestScore
        {
            get { return _bestScore; }
        }

        /// <summary>
        /// 최고 도달 누적 웨이브 번호 (기록 없으면 0)
        /// </summary>
        public int BestCumulativeWaveNumber
        {
            get { return _bestCumulativeWaveNumber; }
        }

        /// <summary>
        /// PlayerPrefs에서 기록을 읽어 둔다. 없으면 0이다.
        /// </summary>
        public RecordStore()
        {
            _bestScore = PlayerPrefs.GetInt(BEST_SCORE_KEY, 0);
            _bestCumulativeWaveNumber = PlayerPrefs.GetInt(BEST_CUMULATIVE_WAVE_KEY, 0);
        }

        /// <summary>
        /// 한 판의 결과를 기존 기록과 각각 비교해 더 높은 값만 갱신하고, 바뀌었으면 디스크에 기록한다.
        /// </summary>
        /// <param name="score">이번 판 점수</param>
        /// <param name="cumulativeWaveNumber">이번 판 도달 누적 웨이브 번호</param>
        /// <returns>점수나 웨이브 중 하나라도 기록을 넘었으면 true</returns>
        public bool TryUpdate(int score, int cumulativeWaveNumber)
        {
            bool isUpdated = false;
            if (score > _bestScore)
            {
                _bestScore = score;
                PlayerPrefs.SetInt(BEST_SCORE_KEY, _bestScore);
                isUpdated = true;
            }
            if (cumulativeWaveNumber > _bestCumulativeWaveNumber)
            {
                _bestCumulativeWaveNumber = cumulativeWaveNumber;
                PlayerPrefs.SetInt(BEST_CUMULATIVE_WAVE_KEY, _bestCumulativeWaveNumber);
                isUpdated = true;
            }

            if (isUpdated)
            {
                PlayerPrefs.Save();
            }
            return isUpdated;
        }
    }
}
