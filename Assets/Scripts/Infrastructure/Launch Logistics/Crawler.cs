using UnityEngine;
using Infrastructure.Roads;
using Infrastructure.Roads.Data;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.InputSystem;

namespace Launch
{
    public class Crawler : MonoBehaviour
    {
        [Header("References")]
        public RoadNetwork roadNetwork;
        public RoadIntersectionComponent start;
        public RoadIntersectionComponent destination;

        [Header("Constants")]
        public float speed = 0.1f;
        public float yLevel = 1.0f;
        public float turnRadius = 0.8f;

        private readonly List<RoadIntersection> _path = new List<RoadIntersection>();
        private int _nextIntersectionIndex = 1;

        private State _currentState = State.NotStarted;

        private Vector3 _position;

        private Vector3 _p0;
        private Vector3 _p1;
        private Vector3 _p2;
        private float _timeSinceTurnBegan = 0.0f;
        private float _turnDuration;

        private void Start()
        {
            _position = transform.position;

            UpdatePath();

            roadNetwork.OnIntersectionAdded += roadIntersection => UpdatePath();
            roadNetwork.OnIntersectionDeleted += roadIntersection => UpdatePath();
            roadNetwork.OnSectionAdded += roadSection => UpdatePath();
            roadNetwork.OnSectionDeleted += roadSection => UpdatePath();
        }

        private void OnEnable()
        {
            UpdatePath();
        }

        private void UpdatePath()
        {
            _path.Clear();

            IEnumerable<RoadSection> path = roadNetwork.GetCrawlerPath(start.RoadIntersection, destination.RoadIntersection);

            if (path.Count() < 1)
            {
                _currentState = State.Blocked;
            } else
            {
                _path.Add(start.RoadIntersection);

                // Create path of intersections
                foreach (RoadSection section in path)
                {
                    if (section.Source == _path.Last())
                        _path.Add(section.Target);
                    else if (section.Target == _path.Last())
                        _path.Add(section.Source);
                    else
                        _currentState = State.Blocked;
                }

                if (_currentState != State.Blocked && _currentState != State.NotStarted)
                {
                    ResetToStart();
                    BeginMoveStraight();
                }
            }
        }

        private void ResetToStart()
        {
            Vector3 newPos = _position = start.RoadIntersection.Position;
            newPos.y = yLevel;
            transform.position = newPos;

            _nextIntersectionIndex = 0;

            _currentState = State.MovingStraight;
        }

        // Update is called once per frame
        void Update()
        {
            switch (_currentState)
            {
                case State.MovingStraight:
                    MoveStraight();
                    break;

                case State.Turning:
                    Turn();
                    break;
            }

            if (Keyboard.current[Key.Enter].isPressed)
            {
                ResetToStart();
                BeginMoveStraight();
            }
        }

        private void BeginMoveStraight()
        {
            _nextIntersectionIndex++;

            // ROTATION
            Vector3 roadVector = _path[_nextIntersectionIndex].Position - _path[_nextIntersectionIndex - 1].Position;
            Vector2 roadVector2D = new Vector2(roadVector.x, roadVector.z);
            float roadAngle = Vector2.SignedAngle(roadVector2D, Vector2.up);
            Vector3 roadRotation = new Vector3(0, roadAngle, 0);
            transform.rotation = Quaternion.Euler(roadRotation);

            _currentState = State.MovingStraight;
        }

        private void MoveStraight()
        {
            RoadIntersection nextIntersection = _path[_nextIntersectionIndex];

            Vector3 newPos = _position = Vector3.MoveTowards(_position, nextIntersection.Position, speed * Time.deltaTime);
            newPos.y = yLevel;
            transform.position = newPos;

            if (_nextIntersectionIndex + 1 == _path.Count)
            {
                if (transform.position == nextIntersection.Position)
                {
                    // This is the last intersection and we've reached it
                    _currentState = State.Ended;
                }
            } else if (Vector3.Distance(_position, nextIntersection.Position) <= turnRadius)
            {
                BeginTurn();
            }
        }

        private void BeginTurn()
        {
            _timeSinceTurnBegan = 0.0f;

            RoadIntersection nextIntersection = _path[_nextIntersectionIndex];

            Vector3 delVector = _path[_nextIntersectionIndex + 1].Position - nextIntersection.Position;

            _p0 = _position;
            _p1 = nextIntersection.Position;
            _p2 = (delVector.normalized * turnRadius) + nextIntersection.Position;

            _turnDuration = CalculateQuadBezierLength(_p0, _p1, _p2) / speed;

            _currentState = State.Turning;
        }

        private void Turn()
        {
            float t = Mathf.Clamp(_timeSinceTurnBegan / _turnDuration, 0.0f, 1.0f);

            Vector3 newPos = _position = CalculateQuadBezierPoint(t, _p0, _p1, _p2);
            newPos.y = yLevel;
            transform.position = newPos;

            Vector3 forwards = CalculateQuadBezierTangent(t, _p0, _p1, _p2);
            transform.rotation = Quaternion.LookRotation(forwards, Vector3.up);

            if (_position == _p2)
            {
                // End turn
                BeginMoveStraight();
            }

            _timeSinceTurnBegan += Time.deltaTime;
        }

        private Vector3 CalculateQuadBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            // https://www.gamasutra.com/blogs/VivekTank/20180806/323709/How_to_work_with_Bezier_Curve_in_Games_with_Unity.php
            float u = 1 - t;

            Vector3 p = u * u * p0;
            p += 2 * u * t * p1;
            p += t * t * p2;

            return p;
        }

        private Vector3 CalculateQuadBezierTangent(float t, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            // https://en.wikipedia.org/wiki/B%C3%A9zier_curve#Quadratic_B%C3%A9zier_curves

            Vector3 B = 2 * t * (p2 - p1);

            B += (2 - (2 * t)) * (p1 - p0);

            return B;
        }

        private float CalculateQuadBezierLength(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            // https://malczak.linuxpl.com/blog/quadratic-bezier-curve-length/

            Vector3 a = p0 - (2 * p1) + p2;
            Vector3 b = (2 * p1) - (2 * p0);

            float A = 4 * (a.x * a.x + a.z * a.z);
            float B = 4 * (a.x * b.x + a.z * b.z);
            float C = b.x * b.x + b.z * b.z;

            float Sabc = 2 * Mathf.Sqrt(A + B + C);
            float A_2 = Mathf.Sqrt(A);
            float A_32 = 2 * A * A_2;
            float C_2 = 2 * Mathf.Sqrt(C);
            float BA = B / A_2;

            float length = A_32 * Sabc;
            length += A_2 * B * (Sabc - C_2);
            length += (4 * C * A - B * B) * Mathf.Log((2 * A_2 + BA + Sabc) / (BA + C_2), (float) Math.E);
            length /= (4 * A_32);

            return length;
        }

        enum State
        {
            NotStarted,
            Blocked,
            MovingStraight,
            Turning,
            Ended
        }
    }
}