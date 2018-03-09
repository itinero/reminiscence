using System;
using System.Collections;
using System.Collections.Generic;

namespace Reminiscence.Indexes
{
    public partial class Index<T>
    {
        private struct Enumerator : IEnumerator<KeyValuePair<long, T>>
        {
            Index<T> _parent;

            public Enumerator(Index<T> parent)
            {
                _parent = parent;
                _accessorIdx = -1;
                _nextId = -1;
                _current = default(KeyValuePair<long, T>);
            }

            int _accessorIdx;
            long _nextId;
            KeyValuePair<long, T>? _current;

            /// <summary>
            /// Gets the current item.
            /// </summary>
            public KeyValuePair<long, T> Current => _current.Value;

            object IEnumerator.Current => _current.Value;

            /// <summary>
            /// Disposes of native resources associated with thie enumerator.
            /// </summary>
            public void Dispose()
            {
                _parent = null;
            }

            /// <summary>
            /// Move to the next item.
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                _current = null;
                if (_nextId == long.MaxValue)
                {
                    return false;
                }

                if (_accessorIdx == -1)
                {
                    _accessorIdx = 0;
                    _nextId = 0;
                }

                // calculate accessor id.
                var a = 0;
                var accessorBytesLostPrevious = 0L;
                var accessorBytesLost = _parent._accessorBytesLost[a];
                var accessorBytesOffset = _parent._accessorSize - accessorBytesLost;
                while (accessorBytesOffset <= _nextId)
                { // keep looping until the accessor is found where the data is located.
                    a++;
                    if (a >= _parent._accessors.Count)
                    {
                        throw new System.Exception("Cannot read elements with an id outside of the accessor range.");
                    }
                    accessorBytesLostPrevious = accessorBytesLost;
                    accessorBytesLost += _parent._accessorBytesLost[a];
                    accessorBytesOffset = (_parent._accessorSize * (a + 1)) - accessorBytesLost;
                }
                var accessor = _parent._accessors[a];
                var accessorOffset = _nextId + accessorBytesLostPrevious - (_parent._accessorSize * a);
                var result = default(T);
                var size = accessor.ReadFrom(accessorOffset, ref result);
                if (size < 0)
                {
                    throw new System.Exception("Failed to read element, perhaps an invalid id was given.");
                }
                _current = new KeyValuePair<long, T>(_nextId, result);

                // update the id.
                _nextId += size;

                // check if we've reached the end.
                if (_nextId + accessorBytesLost >= _parent.SizeInBytes)
                {
                    _nextId = long.MaxValue;
                    return true;
                }

                // check if the id was the last one of the current accessor.
                if (accessorOffset + size + _parent._accessorBytesLost[a] >=
                    accessor.Capacity)
                { // this accessor is at it's end.
                    // check if there is a next acessor.
                    if (a + 1 >= _parent._accessors.Count)
                    {
                        _nextId = long.MaxValue;
                        return true;
                    }

                    // move to the next accessor.
                    a++;
                    _nextId = (_parent._accessorSize * a) - accessorBytesLost;
                }
                return true;
            }

            public void Reset()
            {
                _accessorIdx = -1;
                _nextId = -1;
            }
        }
    }
}