// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 56 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System;

namespace WaterWizard.Client.gamescreen
{
    public class GameTimer
    {
        private float _totalTime;
        private float _remainingTime;
        private bool _isPaused;

        public GameTimer(float initialTime = 60.0f)
        {
            _totalTime = initialTime;
            _remainingTime = initialTime;
            _isPaused = false;
        }

        public float GetRemainingTime()
        {
            return _remainingTime;
        }

        public void Update(float deltaTime)
        {
            if (!_isPaused && _remainingTime > 0)
            {
                _remainingTime -= deltaTime;
                if (_remainingTime < 0)
                {
                    _remainingTime = 0;
                }
            }
        }

        public bool IsTimeUp()
        {
            return _remainingTime <= 0;
        }

        public void Reset()
        {
            _remainingTime = _totalTime;
            _isPaused = false;
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Resume()
        {
            _isPaused = false;
        }
    }
}