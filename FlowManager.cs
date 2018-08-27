using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFlow;

public class FlowManager : MonoBehaviour {

    static FlowManager instance;
    public static FlowManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FlowManager();
            }
            return instance;
        }
    }

    public FlowParser GameFlow;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            GameFlow = new FlowParser();
            GameFlow.LoadNodes(@"[YOURFLOW].xml");

            Debug.Log("FlowManager initialized");

        }
    }

    // Use this for initialization
    void Start () {
        GameFlow.AddCaller(EventQueue.Instance);
        //GameFlow.AddCaller(SoundManager.Instance);
        //GameFlow.AddCaller(GameManager.Instance);
        //(...) more references here

        GameFlow.FireTrigger("GameStart");

    }
	
	// Update is called once per frame
	void Update () {
        //We are currently on a node
        if (GameFlow.IsActive)
        {
            //there's an active root node
            if (GameFlow.CurrentNodeType() == NodeType.RootNode)
                GameFlow.GoToNextNode();

            if (GameFlow.CurrentNodeType() == NodeType.MethodNode || GameFlow.CurrentNodeType() == NodeType.ConditionNode)
            {
                GameFlow.ExecuteCurrentAction();
                GameFlow.GoToNextNode();
            }
        }
    }
}
