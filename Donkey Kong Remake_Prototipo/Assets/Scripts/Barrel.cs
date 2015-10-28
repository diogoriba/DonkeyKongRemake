using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Barrel : MonoBehaviour 
{
    public Waypoint waypointToFollow;
    public float rotationSpeed = 10f;
    public float speed = 10f;
    public bool isStatic = false;
    bool dead;

    private Waypoint lastWaypoint;
    void OnTriggerEnter(Collider other)
    {
        Waypoint waypoint = other.gameObject.GetComponent<Waypoint>();
        if (waypoint != null && waypoint != lastWaypoint)
        {
            ChangeTarget(waypoint);
        }
    }

    void ChangeTarget(Waypoint waypoint)
    {
        lastWaypoint = waypoint;
        if (waypoint.nextWaypoints.Count > 0)
        {
            waypoint.lastChoice = (waypoint.lastChoice + 1) % waypoint.nextWaypoints.Count;
            waypointToFollow = waypoint.nextWaypoints[waypoint.lastChoice];
        }
        else
        {
            waypointToFollow = null;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("MORREU");
        }
    }

    void Burn()
    {
        dead = true;
        collider.enabled = false;
        Destroy(gameObject, 2f);
    }

    void Start()
    {
        dead = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (waypointToFollow != null)
        {
            Vector3 pos = waypointToFollow.transform.position - transform.position;
            pos.y = 0; // force rotation on Y
            Quaternion myRot = Quaternion.LookRotation(pos);
            transform.rotation = Quaternion.Slerp(transform.rotation, myRot, rotationSpeed * Time.deltaTime);

            Vector3 force = transform.forward * (Time.deltaTime * speed);
            Vector3 newPosition = transform.position + force;
            transform.position = newPosition;
        }
        else if (!isStatic)
        {
            Burn();
        }

        if (dead)
        {
            transform.position = transform.position + ((-transform.up) * speed * Time.deltaTime);
        }
    }
}
