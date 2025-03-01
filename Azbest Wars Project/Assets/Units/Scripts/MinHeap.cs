using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct MinHeap
{
    private NativeList<int> heap;
    private NativeList<int> costs;
    public MinHeap(int capacity, Allocator allocator)
    {
        heap = new NativeList<int>(allocator);
        costs = new NativeList<int>(allocator);
    }

    public bool IsEmpty => heap.Length == 0;
    public int Length => heap.Length;
    public bool Contains(int index)
    {
        return heap.Contains(index);
    }

    public void Insert(int nodeIndex, int cost)
    {
        heap.Add(nodeIndex);
        costs.Add(cost);
        HeapifyUp(heap.Length - 1);
    }

    public int ExtractMin()
    {
        int minNode = heap[0];
        heap[0] = heap[heap.Length - 1];
        costs[0] = costs[costs.Length - 1];
        heap.RemoveAt(heap.Length - 1);
        costs.RemoveAt(costs.Length - 1);
        HeapifyDown(0);
        return minNode;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (costs[index] >= costs[parentIndex]) break;
            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void HeapifyDown(int index)
    {
        while (true)
        {
            int leftChild = 2 * index + 1;
            int rightChild = 2 * index + 2;
            int smallest = index;

            if (leftChild < heap.Length && costs[leftChild] < costs[smallest]) smallest = leftChild;
            if (rightChild < heap.Length && costs[rightChild] < costs[smallest]) smallest = rightChild;

            if (smallest == index) break;
            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int i, int j)
    {
        (heap[i], heap[j]) = (heap[j], heap[i]);
        (costs[i], costs[j]) = (costs[j], costs[i]);
    }

    public void Dispose()
    {
        heap.Dispose();
        costs.Dispose();
    }
}