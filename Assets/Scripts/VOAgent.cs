using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class VOAgent : MonoBehaviour
{
    [SerializeField] private VOAgent m_neighbor;
    

    public Vector3 velocity;

    public Vector3 destination;





    //
    private Vector3 disirevelocity;
    public float maxspeed = 2;
    public float radius = 0.5f;


    private KDebugDraw m_debugDraw;

    private int m_hDraw;
	// Use this for initialization
	void Start ()
	{
	    destination = transform.position;

        m_debugDraw = Camera.main.GetComponent<KDebugDraw>();
	    if (m_debugDraw==null)
	    {
	        m_debugDraw = Camera.main.gameObject.AddComponent<KDebugDraw>();
	    }

        m_hDraw = m_debugDraw.addDrawLine();

	}
	
	// Update is called once per frame
	void Update ()
	{
	    if ((destination - transform.position).sqrMagnitude <= 0.01f)
	    {
            velocity = Vector3.zero;
	        return;
	    }
        
        ComputNewVelocity();


        //moving ...
	    transform.position += velocity*Time.deltaTime;
	}

    void ComputNewVelocity()
    {
        m_debugDraw.ClearLine(m_hDraw);
        Vector3 dirWithDistance = destination - transform.position;
        Vector3 desirevelocity = dirWithDistance.normalized;


        //
        Vector3 relativPosition = m_neighbor.transform.position - transform.position;
        Vector3 relativeVelocity = velocity - m_neighbor.velocity;
        float relativeDistanceSq = relativPosition.sqrMagnitude;
        float combinedRadius = radius + m_neighbor.radius;
        
        float cosAngleSq = relativeDistanceSq / (relativeDistanceSq * relativeDistanceSq + combinedRadius * combinedRadius);  //
        float cosVelocity = Vector3.Dot(desirevelocity, relativPosition);







        //for debugdraw
        m_debugDraw.DrawLine(m_hDraw, transform.position, m_neighbor.transform.position, Color.blue);


        velocity = desirevelocity * maxspeed;

        m_debugDraw.DrawLine(m_hDraw, transform.position, transform.position + velocity, Color.green);

    }


    void OnDrawGizmos()
    {
    }
    

}
