﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class JobSerializer
    {
        JobTimer _timer = new JobTimer();
        Queue<IJob> _jobQueue = new Queue<IJob>();
        object _lock = new object();
        bool _flush = false;

        public void PushAfter(int tickAfter, Action action) { PushAfter(tickAfter, new Job(action)); }
        public void PushAfter<T1>(int tickAfter, Action<T1> action, T1 t1) { PushAfter(tickAfter, new Job<T1>(action, t1)); }

        public void PushAfter(int tickAfter, IJob job)
        {
            _timer.Push(job, tickAfter);
        }

        public void Push(Action action) { Push(new Job(action)); }
        public void Push<T1>(Action<T1> action, T1 t1) { Push(new Job<T1>(action, t1)); }
        public void Push<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Push(new Job<T1, T2>(action, t1, t2)); }

        public void Push(IJob job)
        {
            lock (_lock)
            {
                _jobQueue.Enqueue(job);
            }
        }

        public void Flush()
        {
            _timer.Flush();

            while (true)
            {
                IJob job = Pop();
                if (job == null)
                    return;

                job.Execute();
            }
        }

        IJob Pop()
        {
            lock (_lock)
            {
                if (_jobQueue.Count == 0)
                {
                    _flush = false;
                    return null;
                }
                return _jobQueue.Dequeue();
            }
        }
    }
}
