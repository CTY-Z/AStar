using CMCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public class PathRequestManager : CMSingleton<PathRequestManager>
    {
        Queue<PathRequest> queue_pathRequest = new();
        PathRequest curPathRequest;
        
        PathFinding pathFinding;
        bool isProcessingPath;

        protected override void Awake()
        {
            pathFinding = GetComponent<PathFinding>();
        }

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


    }
}



