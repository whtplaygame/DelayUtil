using System.Collections.Generic;

namespace Pool
{
    public class InfoPool<T> where T : PoolInfo
    {
        private readonly Stack<T> _goPool;

        public InfoPool(int capacity = 32)
        {
            _goPool = new Stack<T>(capacity);
        }

        public void Push(T go)
        {
            if (go.InPool)
            {
                return;
            }

            go.InPool = true;
            _goPool.Push(go);
        }

        public T Pop()
        {
            if (_goPool.Count > 0)
            {
                T go = _goPool.Pop();
                go.InPool = false;
                return go;
            }

            return null;
        }

        public int Count()
        {
            return _goPool.Count;
        }

        public void RemoveGc(int leave)
        {
            if (_goPool.Count <= leave)
            {
                return;
            }

            var num = _goPool.Count - leave;
            for (int i = 0; i < num; i++)
            {
                _goPool.Pop();
            }

            System.GC.Collect();
        }
    }
    
    public class PoolInfo
    {
        [JsonIgnore] public bool InPool = false;
    }
}