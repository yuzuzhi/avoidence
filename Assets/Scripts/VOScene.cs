using UnityEngine;
using System.Collections;

public class VOScene : MonoBehaviour
{


    private VOAgent m_selectAgent;


    private int LAYER_PLAYER;
    private int LAYER_GROUND;

    void Awake()
    {
        LAYER_PLAYER = LayerMask.NameToLayer("Player");
        LAYER_GROUND = LayerMask.NameToLayer("Ground");
    }
    // Use this for initialization
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {


        if (Input.GetMouseButtonDown(0))
        {
            do
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (!Physics.Raycast(ray, out hit))
                    break;

                //
                if (hit.collider.gameObject.layer == LAYER_PLAYER)
                {
                    m_selectAgent = hit.collider.gameObject.GetComponent<VOAgent>();
                    break;
                }
                //
                if (hit.collider.gameObject.layer == LAYER_GROUND && m_selectAgent!=null)
                {
                    m_selectAgent.destination = hit.point;
                    break;
                }


            } while (false);
        }
    }
}
