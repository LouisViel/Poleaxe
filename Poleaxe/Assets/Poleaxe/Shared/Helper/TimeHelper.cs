using UnityEngine;

namespace Poleaxe.Helper
{
    public static class TimeHelper
    {
        public static bool IsPaused => pauseState;
        private static bool pauseState = false;
        private static float timeScale = 1f;

        public static void SetSpeed(float speed)
        {
            if (pauseState) SetPause(false);
            Time.timeScale = speed;
        }

        public static void SetPause() => SetPause(!pauseState);
        public static void SetPause(bool state)
        {
            pauseState = state;
            float _timeScale = Time.timeScale;
            Time.timeScale = pauseState ? 0f : timeScale;
            timeScale = _timeScale;
        }
    }
}