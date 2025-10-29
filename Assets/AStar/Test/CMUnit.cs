using CMAStar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMUnit : MonoBehaviour
{
    public Transform target;
    float speed = 20.0f;
    Vector3[] array_path;
    int targetIdx;

    private void Start()
    {
        PathRequestManager.RequestPath(this.transform.position, target.position, OnPathFound);
    }

    public void OnPathFound(Vector3[] path, bool success)
    {
        if (success)
        {
            array_path = path;
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
        }
    }

    IEnumerator FollowPath()
    {
        Vector3 curWaypoint = array_path[0];

        while (true)
        {
            if(transform.position == curWaypoint)
            {
                targetIdx++;
                if (targetIdx >= array_path.Length)
                    yield break;

                curWaypoint = array_path[targetIdx];
            }

            transform.position = Vector3.MoveTowards(transform.position, curWaypoint, speed * Time.deltaTime);
            yield return null;
        }

    }

    public void OnDrawGizmos()
    {
        if(array_path != null)
        {
            for (int i = targetIdx; i < array_path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(array_path[i], Vector3.one);

                if (i == targetIdx)
                    Gizmos.DrawLine(transform.position, array_path[i]);
                else
                    Gizmos.DrawLine(array_path[i - 1], array_path[i]);
            }
        }
    }

}
