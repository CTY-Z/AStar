using CMCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace CMAStar
{
    public struct PathRequest
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
        {
            this.pathStart = pathStart;
            this.pathEnd = pathEnd;
            this.callback = callback;
        }
    }

    public struct PathResult
    {
        public Vector3[] array_path;
        public bool success;
        public Action<Vector3[], bool> callback;

        public PathResult(Vector3[] array_path, bool success, Action<Vector3[], bool> callback)
        {
            this.array_path = array_path;
            this.success = success;
            this.callback = callback;
        }
    }

    public class PathRequestManager : CMSingleton<PathRequestManager>
    {
        Queue<PathResult> queue_result = new();

        PathFinding pathFinding;
        bool isProcessingPath;

        protected override void Awake()
        {
            pathFinding = GetComponent<PathFinding>();
        }

        private void Update()
        {
            if (queue_result.Count > 0)
            {
                int itemInQueue = queue_result.Count;
                lock (queue_result)
                {
                    for (int i = 0; i < itemInQueue; i++)
                    {
                        PathResult result = queue_result.Dequeue();
                        result.callback(result.array_path, result.success);
                    }
                }
            }
        }

        public static void RequestPath(PathRequest request)
        {
            ThreadStart threadStart = delegate
            {
                instance.pathFinding.FindPath(request, instance.FinishedProcessingPath);
            };
            threadStart.Invoke();
        }

        public void FinishedProcessingPath(PathResult result)
        {
            lock (queue_result)
            {
                queue_result.Enqueue(result);
            }
        }

        /*
        Queue<PathRequest> queue_pathRequest = new();
        PathRequest curPathRequest;

        public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
        {
            PathRequest pathRequest = new(pathStart, pathEnd, callback);
            instance.queue_pathRequest.Enqueue(pathRequest);
            instance.TryProcessNext();
        }

        public void FinishedProcessingPath(Vector3[] path, bool success)
        {
            curPathRequest.callback(path, success);
            isProcessingPath = false;
            TryProcessNext();
        }

        private void TryProcessNext()
        {
            if (!isProcessingPath && queue_pathRequest.Count > 0)
            {
                curPathRequest = queue_pathRequest.Dequeue();
                isProcessingPath = true;
                pathFinding.StartFindPath(curPathRequest.pathStart, curPathRequest.pathEnd);
            }
        }
        */

    }
}



