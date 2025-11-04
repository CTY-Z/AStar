using CMAStar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMUnit : MonoBehaviour
{
    const float minPathUpdateTime = .2f;
    const float pathUpdateMoveThreshold = .5f;

    public Transform target;
    public float speed = 20.0f;
    public float turnDst = 5.0f;
    public float turnSpeed = 3f;
    public float stoppingDst = 10;

    CMPath path;

    Vector3[] array_path;
    int targetIdx;

    private void Start()
    {
        StartCoroutine(UpdatePath());
    }

    public void OnPathFound(Vector3[] waypoints, bool success)
    {
        if (success)
        {
            path = new CMPath(waypoints, transform.position, turnDst, stoppingDst);
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
        }
    }

    IEnumerator UpdatePath()
    {
        if(Time.timeSinceLevelLoad < .3f)
            yield return new WaitForSeconds(.3f);

        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);

        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetPosOld = target.position;

        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if((target.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
                targetPosOld = target.position;
            }
        }
    }

    IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIdx = 0;
        transform.LookAt(path.array_lookPoint[0]);

        float speedPercent = 1.0f;

        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);

            //todo:fixed
            if (pathIdx < path.array_turnBoundary.Length)
            {
                while (path.array_turnBoundary[pathIdx].HasCrossedLine(pos2D))
                {
                    if (pathIdx == path.finishLineIdx)
                    {
                        followingPath = false;
                        break;
                    }
                    else
                        pathIdx++;
                }
            }

            if (followingPath)
            {
                if(pathIdx >= path.slowdownIdx && stoppingDst >0)
                {
                    speedPercent = Mathf.Clamp01(path.array_turnBoundary[path.finishLineIdx].DistanceFromPoint(pos2D) / stoppingDst);
                    if (speedPercent < 0.01f)
                        followingPath = false;
                }

                //todo:fixed
                if (pathIdx < path.array_lookPoint.Length)
                {
                    Quaternion q = Quaternion.LookRotation(path.array_lookPoint[pathIdx] - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, q, Time.deltaTime * turnSpeed);
                    transform.Translate(Vector3.forward * Time.deltaTime * speed * speedPercent, Space.Self);
                }
            }

            yield return null;
        }

    }

    public void OnDrawGizmos()
    {
        if(path != null)
            path.DrawWithGizmos();
    }

}
