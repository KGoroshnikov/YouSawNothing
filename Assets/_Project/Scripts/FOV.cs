using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FOV : MonoBehaviour
{
    [SerializeField] private float viewRadius = 10f;
    [SerializeField] private float viewAngle = 90f;
    
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private Vector3 offset;

    public List<Transform> visibleTargets = new List<Transform>();

    void Start()
    {
        InvokeRepeating("FindVisibleTargets", 0.2f, 0.2f);
    }

    void FindVisibleTargets()
    {
        List<Transform> newVisibleTargets = new List<Transform>();

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position + offset, viewRadius, targetMask);
        foreach (Collider targetCollider in targetsInViewRadius)
        {
            Transform target = targetCollider.transform;
            Vector3 dirToTarget = (target.position - transform.position + offset).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position + offset, target.position);
                if (!Physics.Raycast(transform.position + offset, dirToTarget, dstToTarget, obstacleMask))
                {
                    newVisibleTargets.Add(target);
                }
            }
        }

        foreach (Transform target in newVisibleTargets)
        {
            if (!visibleTargets.Contains(target))
            {
                visibleTargets.Add(target);
            }
        }

        for(int i = 0; i < visibleTargets.Count; i++){
            if (!newVisibleTargets.Contains(visibleTargets[i]))
            {
                visibleTargets.Remove(visibleTargets[i]);
                i--;
            }
        }

    }

    public bool isMeVisible(GameObject obj){
        return visibleTargets.Contains(obj.transform);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + offset, viewRadius);

        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2, false);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2, false);
        Gizmos.DrawLine(transform.position + offset, transform.position + offset + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position + offset, transform.position + offset + rightBoundary * viewRadius);

        Gizmos.color = Color.blue;
        foreach (Transform target in visibleTargets)
        {
            Gizmos.DrawLine(transform.position + offset, target.position);
        }
    }
}
