﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public interface IJob
    {
        void Execute();
    }

    public class Job : IJob
    {
        Action _action;

        public Job(Action action)
        {
            _action = action;
        }

        public void Execute()
        {
            _action.Invoke();
        }
    }

    public class Job<T1> : IJob
    {
        Action<T1> _action;
        T1 _t1;

        public Job(Action<T1> action, T1 t1)
        {
            _action = action;
            _t1 = t1;
        }

        public void Execute()
        {
            _action.Invoke(_t1);
        }
    }

    public class Job<T1, T2> : IJob
    {
        Action<T1, T2> _action;
        T1 _t1;
        T2 _t2;

        public Job(Action<T1, T2> action, T1 t1, T2 t2)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
        }

        public void Execute()
        {
            _action.Invoke(_t1, _t2);
        }
    }
}
