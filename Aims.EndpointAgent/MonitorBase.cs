using System;
using System.Diagnostics;
using System.Threading;

namespace Aims.EndpointAgent
{
    public abstract class MonitorBase<T> : IDisposable
    {
        private readonly int _intervalMilliseconds;

        protected bool _isRunning = true;

        protected MonitorBase(int intervalMilliseconds, bool manualStart = false)
        {
            _intervalMilliseconds = intervalMilliseconds;

            if (!manualStart)
            {
                Start();
            }
        }

        public virtual void Dispose()
        {
            _isRunning = false;
        }

        protected abstract T[] Collect();

        protected abstract void Send(T[] items);

        protected virtual void Start()
        {
            var thread = new Thread(Run) { IsBackground = true };
            thread.Start();
        }

        protected void Run()
        {
            var stopwatch = new Stopwatch();
            while (_isRunning)
            {
                stopwatch.Restart();

                T[] items = Collect();
                if (items.Length > 0)
                {
                    Send(items);
                }

                long timeout = _intervalMilliseconds - stopwatch.ElapsedMilliseconds;
                Thread.Sleep(timeout > 0 ? (int)timeout : 0);
            }
        }
    }
}