using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<T> heap;
    private IComparer<T> comparer;

    // 기본 생성자: 기본 비교자를 사용합니다.
    public PriorityQueue() : this(Comparer<T>.Default)
    {
    }

    // 커스텀 비교자를 사용하는 생성자
    public PriorityQueue(IComparer<T> comparer)
    {
        if (comparer == null)
            throw new ArgumentNullException(nameof(comparer));
            
        this.comparer = comparer;
        this.heap = new List<T>();
    }

    // 큐에 들어있는 요소의 개수
    public int Count => heap.Count;

    public bool Remove(T item)
    {
        return heap.Remove(item);
    }

    // 새로운 요소를 큐에 추가합니다.
    public void Enqueue(T item)
    {
        heap.Add(item);
        HeapifyUp(heap.Count - 1);
    }

    // 가장 높은 우선순위의 요소를 반환하고 큐에서 제거합니다.
    public T Dequeue()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("The priority queue is empty.");

        T root = heap[0];
        T lastItem = heap[heap.Count - 1];
        heap.RemoveAt(heap.Count - 1);

        if (heap.Count > 0)
        {
            heap[0] = lastItem;
            HeapifyDown(0);
        }
        return root;
    }

    // 가장 높은 우선순위의 요소를 제거하지 않고 반환합니다.
    public T Peek()
    {
        if (heap.Count == 0)
            throw new InvalidOperationException("The priority queue is empty.");
        return heap[0];
    }

    // 힙의 특성을 유지하면서 요소를 위로 올립니다.
    private void HeapifyUp(int index)
    {
        int parentIndex = (index - 1) / 2;

        while (index > 0 && comparer.Compare(heap[index], heap[parentIndex]) < 0)
        {
            Swap(index, parentIndex);
            index = parentIndex;
            parentIndex = (index - 1) / 2;
        }
    }

    // 힙의 특성을 유지하면서 요소를 아래로 내립니다.
    private void HeapifyDown(int index)
    {
        int smallest = index;
        int leftChild = 2 * index + 1;
        int rightChild = 2 * index + 2;

        if (leftChild < heap.Count && comparer.Compare(heap[leftChild], heap[smallest]) < 0)
        {
            smallest = leftChild;
        }

        if (rightChild < heap.Count && comparer.Compare(heap[rightChild], heap[smallest]) < 0)
        {
            smallest = rightChild;
        }

        if (smallest != index)
        {
            Swap(index, smallest);
            HeapifyDown(smallest);
        }
    }

    // 두 인덱스의 요소를 교환합니다.
    private void Swap(int i, int j)
    {
        T temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;
    }
}
