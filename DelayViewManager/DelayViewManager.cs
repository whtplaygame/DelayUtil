using System;
using System.Collections.Generic;
using Log;
using Pool;
using UnityEngine;

namespace Delay
{
    public class DelayViewManager : MonoBehaviour
    {
        public static DelayViewManager Instance { get; private set; }

        public static void Create(GameObject go)
        {
            Instance = go.AddComponent<DelayViewManager>();
            Instance.Init();
        }

        public static void Destroy()
        {
            Instance.Release();
            GameObject.Destroy(Instance);
            Instance = null;
        }

        private DelayViewManager()
        {
        }

        private void Init()
        {
        }

        private void Release()
        {
        }

        public void Reload()
        {
        }


        // ---------------------------------------------------------------------------------------------
        // Logic
        // ---------------------------------------------------------------------------------------------

        private List<DelayEntity> _delayList = new List<DelayEntity>(128);
        private List<DelayEntity> _waitList = new List<DelayEntity>(128);
        private float _timer;

        public void AddDelay(DelayEntity entity)
        {
            entity.Delay += _timer;
            _waitList.Add(entity);
        }

        public void RemoveDelay(DelayEntity entity)
        {
            entity.IsEnd = true;
        }

        private void ResetTimer()
        {
            _timer = 0;
        }

        private void Update()
        {
            if (_waitList.Count <= 0 && _delayList.Count <= 0)
            {
                return;
            }

            if (_waitList.Count > 0)
            {
                if (_delayList.Count > 0)
                {
                    foreach (var entity in _waitList)
                    {
                        var delay = entity.Delay;
                        if (delay < _delayList[_delayList.Count - 1].Delay)
                        {
                            _delayList.Add(entity);
                        }
                        else if (delay > _delayList[0].Delay)
                        {
                            _delayList.Insert(0, entity);
                        }
                        else
                        {
                            var pos = FindPos(entity);
#if UNITY_EDITOR
                            var e = _delayList[pos];
                            if (e.Delay > entity.Delay)
                            {
                                D.Error($"error delay :  {e.Delay}, {entity.Delay}");
                            }
#endif

                            _delayList.Insert(pos, entity);
                        }
                    }

                    _waitList.Clear();
                }
                else
                {
                    _waitList.Sort(Cmp);
                    for (int i = 0; i < _waitList.Count; i++)
                    {
                        _delayList.Add(_waitList[i]);
                    }

                    _waitList.Clear();
                }
            }

            _timer += Time.deltaTime;

            while (_delayList.Count > 0)
            {
                var entity = _delayList[_delayList.Count - 1];
                if (entity.IsEnd)
                {
                    _delayList.RemoveAt(_delayList.Count - 1);
                    Push(entity);
                }
                else if (entity.Delay < _timer)
                {
                    _delayList.RemoveAt(_delayList.Count - 1);
                    entity.IsEnd = true;
                    entity.Callback?.Invoke();
                    Push(entity);
                }
                else
                {
                    break;
                }
            }

            if (_delayList.Count <= 0 && _waitList.Count <= 0)
            {
                ResetTimer();
            }
        }

        private int FindPos(DelayEntity entity)
        {
            var start = 0;
            var end = _delayList.Count - 1;
            while (end - start > 1)
            {
                var middle = (start + end) / 2;
                var e = _delayList[middle];
                if (e.Delay < entity.Delay)
                {
                    end = middle;
                }
                else
                {
                    start = middle;
                }
            }

            if (entity.Delay >= _delayList[start].Delay)
            {
                return start;
            }
            else if (entity.Delay >= _delayList[end].Delay)
            {
                return end;
            }
            else
            {
                return end + 1;
            }
        }

        private int Cmp(DelayEntity a, DelayEntity b)
        {
            // if (Mathf.Abs(a.Delay - b.Delay) <= 0.0001f)
            // {
            //     return a.Uid < b.Uid ? 1 : -1;
            // }

            return a.Delay <= b.Delay ? 1 : -1;
        }


        // ---------------------------------------------------------------------------------------------
        // cache
        // ---------------------------------------------------------------------------------------------

        private static readonly InfoPool<DelayEntity> _pool = new InfoPool<DelayEntity>();

        public void Push(DelayEntity entity)
        {
            if (entity.InPool)
            {
                return;
            }

            entity.Clear();
            _pool.Push(entity);
        }

        public DelayEntity Pop(float delay, Action callback)
        {
            var entity = _pool.Pop();
            if (entity == null)
            {
                entity = new DelayEntity();
            }

            entity.Clear();
            entity.Callback = callback;
            entity.Delay = delay;
            entity.Uid = DelayEntity.GenerateUuid();
            return entity;
        }

        public DelayEntity Delay(float delay, Action callback, int id = -1)
        {
            var delayEntity = Pop(delay, callback);
            if (id > 0)
            {
                delayEntity.Uid = id;
            }

            AddDelay(delayEntity);
            return delayEntity;
        }

        public void RemoveGc()
        {
            _pool.RemoveGc(64);
        }
    }
}