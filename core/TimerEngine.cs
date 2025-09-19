namespace Timer.core
{
    public enum TimerMode { Countdown, Stopwatch }

    public sealed class TimerEngine
    {
        public TimerMode Mode { get; private set; } = TimerMode.Countdown;
        public bool Running { get; private set; }
        public int ShownSeconds { get; private set; }

        int _baseSeconds;
        DateTime? _startUtc;
        DateTime? _targetUtc;

        public void SetMode(TimerMode mode)
        {
            if (Running) throw new InvalidOperationException("실행 중에는 모드를 바꿀 수 없습니다.");
            Mode = mode;
            Reset();
        }

        public void StartCountdown(int seconds)
        {
            if (seconds <= 0) throw new ArgumentOutOfRangeException(nameof(seconds));
            _targetUtc = DateTime.UtcNow.AddSeconds(seconds);
            ShownSeconds = seconds;
            Running = true;
            _startUtc = null;
        }

        public void StartStopwatch(int startSeconds)
        {
            _baseSeconds = Math.Max(0, startSeconds);
            _startUtc = DateTime.UtcNow;
            ShownSeconds = _baseSeconds;
            Running = true;
            _targetUtc = null;
        }

        public void Pause()
        {
            if (!Running) return;
            Running = false;

            var now = DateTime.UtcNow;

            if (Mode == TimerMode.Countdown)
            {
                if (_targetUtc is not null)
                {
                    int remain = Math.Max(0, (int)Math.Ceiling((_targetUtc.Value - now).TotalSeconds));
                    ShownSeconds = remain;
                }
                _targetUtc = null;
            }
            else
            {
                if (_startUtc is not null)
                {
                    _baseSeconds += Math.Max(0, (int)Math.Floor((now - _startUtc.Value).TotalSeconds));
                    ShownSeconds = _baseSeconds;
                }
                _startUtc = null;
            }
        }

        public void Reset()
        {
            Running = false;
            ShownSeconds = 0;
            _baseSeconds = 0;
            _startUtc = null;
            _targetUtc = null;
        }

        public (bool beep, bool stopped) Tick(DateTime nowUtc)
        {
            if (!Running) return (false, false);

            if (Mode == TimerMode.Countdown)
            {
                if (_targetUtc is null)
                {
                    Running = false; return (false, true);
                }

                int remain = (int)Math.Ceiling((_targetUtc.Value - nowUtc).TotalSeconds);
                if (remain <= 0)
                {
                    ShownSeconds = 0;
                    Running = false;
                    _targetUtc = null;
                    return (true, true);
                }
                ShownSeconds = remain;
                return (false, false);
            }
            else // Stopwatch
            {
                if (_startUtc is null)
                {
                    Running = false; return (false, true);
                }
                int elapsed = Math.Max(0, (int)Math.Floor((nowUtc - _startUtc.Value).TotalSeconds));
                ShownSeconds = _baseSeconds + elapsed;
                return (false, false);
            }
        }
    }
}