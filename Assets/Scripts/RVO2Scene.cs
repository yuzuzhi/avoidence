using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RVO;
using UnityEngine.UI;
using Vector2 = RVO.Vector2;

public class RVO2Scene : MonoBehaviour {


    enum MouseFunc
    {
        AddAgent,
        AddObstacle,
        MovAgent,
    }
	System.Random random;

    [SerializeField]
    Object m_playerPrefabs;
    [SerializeField]
	MouseFunc m_mouseFunc = MouseFunc.AddAgent;

    [SerializeField] private Dropdown m_dropDown;

	Dictionary<int,Transform> m_agentIdlist = new Dictionary<int,Transform>();

    int m_selectedSvoAgent = 0;

    private RVO2Agent m_selectedAgent;

    private KDebugDraw m_debugDraw;
    private int m_hDrawPointList;
    private int m_hDrawObstacle;

	// Use this for initialization
	void Start ()
	{
	    m_debugDraw = Camera.main.gameObject.AddComponent<KDebugDraw>();

		random = new System.Random ();

        //RVO.Simulator.Instance.setTimeStep(0.25f);
        RVO.Simulator.Instance.setAgentDefaults(15.0f, 10, 5.0f, 5.0f, 2.0f, 1, new RVO.Vector2(0.0f, 0.0f));

        m_dropDown.ClearOptions();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        options.Add(new Dropdown.OptionData(MouseFunc.AddAgent.ToString()));
        options.Add(new Dropdown.OptionData(MouseFunc.AddObstacle.ToString()));
        options.Add(new Dropdown.OptionData(MouseFunc.MovAgent.ToString()));
        m_dropDown.AddOptions(options);
	    m_dropDown.value = 0;


        m_hDrawObstacle = m_debugDraw.addDrawLine();
        m_hDrawPointList = m_debugDraw.addDrawLine();

	    m_selectCollider = gameObject.GetComponent<BoxCollider>();

	}


    private List<RVO.Vector2> m_pointList = new List<Vector2>();

    List<int> m_obstacleList = new List<int>();

    private Vector3 m_min;
    private Vector3 m_max;

    private BoxCollider m_selectCollider;
	// Update is called once per frame
	void Update ()
	{
	    //RVO.Simulator.Instance.setTimeStep(Time.deltaTime);
        //if(RVO.Simulator.Instance.getNumAgents()== MAX_AGENT)
        RVO.Simulator.Instance.doStep();

        foreach (var AgentItem in m_agentIdlist)
        {
            RVO.Vector2 pos = RVO.Simulator.Instance.getAgentPosition(AgentItem.Key);
            RVO.Vector2 vel = RVO.Simulator.Instance.getAgentVelocity(AgentItem.Key);
            Vector3 gmPos = AgentItem.Value.position;
            gmPos.x = pos.x_; gmPos.z = pos.y_;
            AgentItem.Value.position = gmPos;
        }

        for(int i=0; i<m_obstacleList.Count;++i)
        {

            //Simulator.Instance.getNextObstacleVertexNo(i);
            int hObs = i;
            RVO.Vector2 v0;
            RVO.Vector2 v1;
            Vector3 from;
            Vector3 to;
            from.y = to.y = 0.1f;
            do
            {
                int next = Simulator.Instance.getNextObstacleVertexNo(hObs);
                v0 = Simulator.Instance.getObstacleVertex(hObs);
                v1 = Simulator.Instance.getObstacleVertex(next);

                from.x = v0.x_; from.z = v0.y_;
                to.x = v1.x_; to.z = v1.y_;
                m_debugDraw.DrawLine(m_hDrawObstacle, from, to, Color.red);

                hObs = Simulator.Instance.getNextObstacleVertexNo(hObs);
            }
            while (hObs != i);
        }


	    m_debugDraw.ClearLine(m_hDrawPointList);
        Vector3 lineFrom;
	    Vector3 lineTo;
	    lineFrom.y = lineTo.y = 0.1f;
	    int pointCount = m_pointList.Count;
        for (int i=0; i< pointCount; ++i)
        {
            RVO.Vector2 v0 = m_pointList[i];
            RVO.Vector2 v1 = (i + 1) < pointCount ? m_pointList[i + 1] : m_pointList[0];
            lineFrom.x  = v0.x_; lineFrom.z = v0.y_;
            lineTo.x    = v1.x_; lineTo.z   = v1.y_;
            m_debugDraw.DrawLine(m_hDrawPointList, lineFrom, lineTo, Color.blue);
        }


        if (Input.GetMouseButtonDown(0))
        {
            do {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (!Physics.Raycast(ray, out hit))
                    break;
                if(m_mouseFunc==MouseFunc.AddAgent)
                {
                    if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
                        break;

                    GameObject gmAgent = Instantiate(m_playerPrefabs, hit.point, Quaternion.identity) as GameObject;
                    m_agentIdlist.Add(gmAgent.GetComponent<RVO2Agent>().RVOAgentHandle, gmAgent.transform);
                }

                if (m_mouseFunc == MouseFunc.AddObstacle)
                {
                    RVO.Vector2 hitPos = new RVO.Vector2(hit.point.x, hit.point.z);

                    if (m_pointList.Count > 2)
                    {
                        RVO.Vector2 last = m_pointList[m_pointList.Count-1];
                        if (RVO.RVOMath.absSq(hitPos - last) <= 0.05f)
                        {
                            int hObstacle = RVO.Simulator.Instance.addObstacle(m_pointList);
                            m_obstacleList.Add(hObstacle); 
                            m_pointList.Clear();
                            RVO.Simulator.Instance.processObstacles();
                            break;
                        }
                    }

                    m_pointList.Add(hitPos);
                }

                if (m_mouseFunc==MouseFunc.MovAgent)
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        if(m_selectedAgent)
                            m_selectedAgent.SetDestPos(hit.point);
                        break;
                    }
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                        m_selectedAgent = hit.collider.gameObject.GetComponent<RVO2Agent>();
                }

            } while (false);
        }

        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);

            Vector3 MAX_VECTOR = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            if (m_min== MAX_VECTOR)
            {
                m_min = hit.point;
            }

            m_max = hit.point;

            if (m_selectCollider.enabled==false && (m_max - m_min).sqrMagnitude > 1.0f)
                m_selectCollider.enabled = true;

            if (m_selectCollider != null)
            {
                m_selectCollider.center = Vector3.Lerp(m_min, m_max, 0.5f);
                Vector3 delta = m_max - m_min;
                m_selectCollider.size = new Vector3(Mathf.Abs(delta.x), 2.0f, Mathf.Abs(delta.z));
            }
        }
        else
        {
            m_min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            m_max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            m_selectCollider.enabled = false;
        }



    }

    public void OnDropDownValueChng()
    {
        m_mouseFunc = (MouseFunc) m_dropDown.value;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 from, to;
        from.y = to.y = 0.1f;

        from.x = m_min.x;
        to.x = m_max.x;
        from.z = to.z = m_min.z;
        Gizmos.DrawLine(from, to);
        from.z = to.z = m_max.z;
        Gizmos.DrawLine(from, to);

        from.z = m_min.z;
        to.z = m_max.z;
        from.x = to.x = m_min.x;
        Gizmos.DrawLine(from, to);
        from.x = to.x = m_max.x;
        Gizmos.DrawLine(from, to);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            RVO2Agent agent = other.GetComponent<RVO2Agent>();
            if (agent != null)
            {
                Debug.Log(other.gameObject.name);
            }

        }
    }
}
