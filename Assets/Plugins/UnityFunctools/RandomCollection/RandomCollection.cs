using System.Collections.Generic;
using System.Linq;
using System;

namespace UnityFunctools
{
    public struct RandomCollection<T>
    {
        private T[] _valuesArray;

        private List<int> _unusedIndexes;

        private bool _isLoop;

        public RandomCollection(IEnumerable<T> collection, bool isLoop = true)
        {
            _valuesArray = collection.ToArray();

            _unusedIndexes = new List<int>(_valuesArray.Length);
            _unusedIndexes.AddRange(Enumerable.Range(0, _valuesArray.Length));

            _isLoop = isLoop;
        }

        public T GetNext()
        {
            if (_unusedIndexes.Count == 0)
            {
                if (_isLoop == false)
                    throw new Exception();

                _unusedIndexes = new List<int>(_valuesArray.Length);
                _unusedIndexes.AddRange(Enumerable.Range(0, _valuesArray.Length));
            }

            int nextIndex = UnityEngine.Random.Range(0, _unusedIndexes.Count);
            T nextValue = _valuesArray[_unusedIndexes[nextIndex]];
            _unusedIndexes.RemoveAt(nextIndex);
            return nextValue;
        }
    }
}