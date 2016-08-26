using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RVO2Agent : MonoBehaviour
{
    [SerializeField] private Animator m_animator;
    private const int WALKABLE = 1;
    private int m_agentHandle;

    private Vector3 m_destPos;

    private int m_cornerIndex;

    private int m_agentMaxNeighbors;

    private NavMeshPath path = new NavMeshPath();

    private bool m_havValidPath;


    // Use this for initialization
    void Awake ()
	{
	    RVO.Vector2 agentPos = new RVO.Vector2(transform.position.x, transform.position.z);
        m_agentHandle = RVO.Simulator.Instance.addAgent(agentPos);
	    RVO.Simulator.Instance.setAgentRadius(m_agentHandle, 0.5f);
        gameObject.name = m_agentHandle.ToString();

	    m_destPos = transform.position;
        m_agentMaxNeighbors = RVO.Simulator.Instance.getAgentMaxNeighbors(m_agentHandle);

	}

    void Start()
    {
        Stop();
    }
    
    public int RVOAgentHandle{get { return m_agentHandle; }}

    public void SetDestPos(Vector3 dest)
    {
        RVO.Simulator.Instance.setAgentMaxNeighbors(m_agentHandle, m_agentMaxNeighbors);

        Vector3 from = transform.position + (dest - transform.position).normalized*0.1f;

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(from, out hit, 1.1f, WALKABLE))
            return;
        
        if (NavMesh.CalculatePath(hit.position, dest, WALKABLE, path))
        {
            Vector3 dist = dest - path.corners[path.corners.Count() - 1];
            if (dist.sqrMagnitude >= 0.01f)
            {
                path.ClearCorners();
                return;
            }

            m_havValidPath = true;
            m_cornerIndex = 1;
            m_destPos = path.corners[m_cornerIndex];
            GetComponent<NavMeshObstacle>().carving = false;
            if(m_animator!=null)
                m_animator.Play("Run", 0);
        }
    }

    private void Stop()
    {
        RVO.Simulator.Instance.setAgentPrefVelocity(m_agentHandle, new RVO.Vector2(0, 0));
        RVO.Simulator.Instance.setAgentMaxNeighbors(m_agentHandle, 0);
        GetComponent<NavMeshObstacle>().carving = true;
        m_havValidPath = false;
        if(m_animator!=null)
            m_animator.Play("Idle", 0);

    }

    // Update is called once per frame
    void Update ()
    {

        if ((m_destPos - transform.position).sqrMagnitude <= 0.1)
        {
            if (m_cornerIndex == path.corners.Count() - 1 || path.corners.Count()==0)
                Stop();
            else
            {
                m_cornerIndex++;
                m_destPos = path.corners[m_cornerIndex];
            }
            return;
        }

        //moving...
        Vector3 dir = m_destPos - transform.position;
        RVO.Vector2 rdir = new RVO.Vector2(dir.x, dir.z);
        rdir = RVO.RVOMath.normalize(rdir);
        RVO.Simulator.Instance.setAgentPrefVelocity(m_agentHandle, rdir);
        RVO.Vector2 velocityInSimulator = RVO.Simulator.Instance.getAgentVelocity(m_agentHandle);
        transform.forward = new Vector3(velocityInSimulator.x_, 0, velocityInSimulator.y_).normalized;
    }


    void OnDrawGizmos()
    {
        IList<RVO.Line> lines = RVO.Simulator.Instance.getAgentOrcaLines(m_agentHandle);


        Gizmos.color = Color.red;
        Vector3 from;
        Vector3 to;
        from.y = to.y = 1.0f;  
        foreach (RVO.Line line in lines)
        {
            from.x = line.point.x_;// - line.direction.x_ * 10;
            from.z = line.point.y_;// - line.direction.y_ * 10;

            to.x = line.point.x_ + line.direction.x_*10;
            to.z = line.point.y_ + line.direction.y_*10;
            Gizmos.DrawLine(from+transform.position,to);
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < path.corners.Count()-1; i++)
        {
            from = path.corners[i];
            to = path.corners[i + 1];

            Gizmos.DrawLine(from, to);

        }


    }
}
