using System;

namespace TnT.Systems.TimeSystem
{
    public abstract class Timer
    {
        protected double initialTime;
        public double Time { get; set; }
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// <see cref="Time"/> as a fraction of the initial time (a countdown: 1 at the start, 0 when finished).
        /// 0 when the initial time is 0 or less (no length; was a division by zero: infinity or NaN).
        /// </summary>
        public double Progress => initialTime > 0 ? Time / initialTime : 0;

        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };

        protected Timer(double value)
        {
            initialTime = value;
            IsRunning = false;
        }

        public void Start()
        {
            Time = initialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                OnTimerStart.Invoke();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                OnTimerStop.Invoke();
            }
        }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        public abstract void Tick(double deltaTime);
    }

    public class CountdownTimer : Timer
    {
        public CountdownTimer(double value) : base(value) { }

        public override void Tick(double deltaTime)
        {
            if (IsRunning && Time > 0)
            {
                Time -= deltaTime;
            }

            if (IsRunning && Time <= 0)
            {
                Stop();
            }
        }

        public bool IsFinished => Time <= 0;

        public void Reset() => Time = initialTime;

        public void Reset(double newTime)
        {
            initialTime = newTime;
            Reset();
        }
    }
}