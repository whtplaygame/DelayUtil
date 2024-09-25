using System;
using System.Diagnostics.Contracts;
using Log;
using Pool;

namespace Delay
{
    public class DelayEntity : PoolInfo
    {
        private static int _uuid = 0;
        public int Uid;
        public Action Callback;
        public float Delay;
        public bool IsEnd;

        public void Clear()
        {
            Callback = null;
            Delay = -1;
            IsEnd = false;
            Uid = 0;
        }

        public void Kill()
        {
            //D.Error($"kill id : {Uid}");
            IsEnd = true;
        }

        public bool HasEnd()
        {
            return IsEnd;
        }

        public bool IsValid(int id)
        {
            return id == Uid;
        }

        public static int GenerateUuid()
        {
            return _uuid++;
        }
    }
}