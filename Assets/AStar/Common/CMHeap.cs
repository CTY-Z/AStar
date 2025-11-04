using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMAStar
{
    public class CMHeap<T> where T : ICMHeapItem<T>
    {
        T[] array_item;

        public int curItemCount;
        public int Count { get { return curItemCount; } } 

        public CMHeap(int maxHeapSize)
        {
            array_item = new T[maxHeapSize];
        }

        public void Add(T item)
        {
            item.heapIdx = curItemCount;
            array_item[curItemCount] = item;
            SortUp(item);
            curItemCount++;
        }

        public T RemoveFirst()
        {
            T firstItem = array_item[0];
            curItemCount--;
            array_item[0] = array_item[curItemCount];
            array_item[0].heapIdx = 0;
            SortDown(array_item[0]);

            return firstItem;
        }

        public bool Contains(T item)
        {
            return Equals(array_item[item.heapIdx], item);
        }

        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        private void SortDown(T item)
        {
            while (true)
            {
                int childIdxL = item.heapIdx * 2 + 1;
                int childIdxR = item.heapIdx * 2 + 2;
                int swapIdx = 0;

                if (childIdxL < curItemCount)
                {
                    swapIdx = childIdxL;
                    if (childIdxR < curItemCount)
                    {
                        if (array_item[childIdxL].CompareTo(array_item[childIdxR]) < 0)
                            swapIdx = childIdxR;
                    }

                    if (item.CompareTo(array_item[swapIdx]) < 0)
                        Swap(item, array_item[swapIdx]);
                    else
                        return;
                }
                else
                    return;
            }
        }

        private void SortUp(T item)
        {
            int parentIdx = (item.heapIdx - 1) / 2;

            while(true)
            {
                T parentItem = array_item[parentIdx];
                if (item.CompareTo(parentItem) > 0)
                    Swap(item, parentItem);
                else
                    break;

                parentIdx = (item.heapIdx - 1) / 2;
            }
        }

        private void Swap(T itemA, T itemB)
        {
            array_item[itemA.heapIdx] = itemB;
            array_item[itemB.heapIdx] = itemA;

            int temp = itemA.heapIdx;
            itemA.heapIdx = itemB.heapIdx;
            itemB.heapIdx = temp;
        }
    }

    public interface ICMHeapItem<T> : IComparable<T>
    {
        int heapIdx { get; set; }

    }
}
