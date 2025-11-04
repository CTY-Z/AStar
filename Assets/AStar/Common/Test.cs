using CMAStar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    int[] a = new int[10];

    // Start is called before the first frame update
    void Start()
    {
        a[0] = 3;
        a[1] = 5;
        a[2] = 6;
        a[3] = 3;
        a[4] = 4;
        a[5] = 5;
        a[6] = 2;
        a[7] = 6;
        a[8] = 9;
        a[9] = 0;

        QS(a, 0, a.Length - 1);

        Debug.Log(a[0]);
        Debug.Log(a[1]);
        Debug.Log(a[2]);
        Debug.Log(a[3]);
        Debug.Log(a[4]);
        Debug.Log(a[5]);
        Debug.Log(a[6]);
        Debug.Log(a[7]);
        Debug.Log(a[8]);
        Debug.Log(a[9]);
    }

    void QS(int[] array_int, int l, int r)
    {
        if (array_int.Length < 0 || array_int.Length == 1)
            return;

        if(l < r)
        {
            int ranIdx = Random.Range(0, array_int.Length - 1);
            Swap(array_int, l + ranIdx, r);

            int[] p = Sort(array_int, l, r);
            QS(array_int, l, p[0] - 1);
            QS(array_int, p[1] + 1, r);
        }
    }

    int[] Sort(int[] array_int, int l, int r)
    {
        int lessBoundary = l - 1;
        int moreBoundary = r;

        while(l < moreBoundary)
        {
            if (array_int[l] < array_int[r])
            {
                lessBoundary++;
                Swap(array_int, lessBoundary, l);
                l++;
            }
            else if(array_int[l] > array_int[r])
            {
                moreBoundary--;
                Swap(array_int, moreBoundary, l);
            }
            else
                l++;
        }

        Swap(array_int, moreBoundary, r);
        return new int[2] { lessBoundary + 1, moreBoundary };
    }

    void Swap(int[] array, int x, int y)
    {
        if (x < 0 || y < 0 || x >= array.Length || y >= array.Length)
            return;

        int temp = array[x];
        array[x] = array[y];
        array[y] = temp;
    }


}

