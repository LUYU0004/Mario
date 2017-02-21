using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : MonoBehaviour
{

    public enum FollowType
    {
        MoveTowards,
        Lerp
    }

    public FollowType Type = FollowType.MoveTowards; //default
    public PathDefinition Path;
    public float speed = 1;
    public float MaxDistanceToGoal = .1f;

    private IEnumerator<Transform> _currentPoint;

    public void Start()
    {
        if (Path == null)
        {
            Debug.LogError("Path cannot be null", gameObject);
            return;
        }

        _currentPoint = Path.GetPathsEnumerator();
        _currentPoint.MoveNext();

        if (_currentPoint.Current == null)
            return;

        //set the object's position to the current point
        transform.position = _currentPoint.Current.position;
    }

    public void Update()
    {
        if (_currentPoint == null || _currentPoint.Current == null)
            return;

        if (Type == FollowType.MoveTowards)
            //MoveTowards(curPosition, toPosition, Max distance Delta)
            transform.position = Vector3.MoveTowards(transform.position, _currentPoint.Current.position, Time.deltaTime * speed);
        else if (Type == FollowType.Lerp)
            transform.position = Vector3.Lerp(transform.position, _currentPoint.Current.position, Time.deltaTime * speed);

        var distanceSquared = (transform.position - _currentPoint.Current.position).sqrMagnitude;
        //x^2 + y^2 + z^2
        if (distanceSquared < MaxDistanceToGoal * MaxDistanceToGoal)
            _currentPoint.MoveNext();
        //invoke the enumerator , as the position is close enough to the next point which can be skipped in this way
    }

}
