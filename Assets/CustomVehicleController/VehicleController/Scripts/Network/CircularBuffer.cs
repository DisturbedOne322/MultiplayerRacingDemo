using UnityEngine;

public class CircularBuffer<T>
{
    private T[] _buffer;
    private int _bufferSize;

    public CircularBuffer(int size)
    {
        _bufferSize = size;
        _buffer = new T[size];
    }

    public void Add(T item, int index) => _buffer[index % _bufferSize] = item;
    public T Get(int index) => _buffer[index % _bufferSize];
    public void Clear() => _buffer = new T[_bufferSize];
}
