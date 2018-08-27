using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Event
{
    public bool IsFinished = false;
    public bool IsStarted = false;

    public Event NextEvent;

    public virtual void EventStart()
    {
        IsFinished = false;
        IsStarted = true;

        NextEvent = null;
    }

    public virtual void Update()
    {
    }

    public virtual void EventEnd()
    {
    }
}

public class ActionEvent : Event
{
    public Action MethodToFire;

    public ActionEvent()
    {

    }
    public ActionEvent(Action action)
    {
        this.MethodToFire = action;
    }

    public override void EventStart()
    {
        base.EventStart();
        MethodToFire.Invoke();
        EventEnd();
    }
}



public class ChangeSceneEvent : Event
{
    public string SceneToLoad;
    public ChangeSceneEvent(string sceneName)
    {
        this.SceneToLoad = sceneName;
    }

    public override void EventStart()
    {
        base.EventStart();
        SceneManager.LoadScene(SceneToLoad);
        EventEnd();
    }
}

public class WaitEvent : Event
{
    public float WaitTime;

    public WaitEvent()
    {
        WaitTime = 0f;
    }

    public WaitEvent(float waitDuration)
    {
        WaitTime = waitDuration;
    }

    public override void Update()
    {
        WaitTime -= Time.deltaTime;
        Debug.Log("Wait event running");

        if (WaitTime <= 0)
            EventEnd();
    }
}


public class EventQueue : MonoBehaviour {

    public Queue<Event> gameEvents;
    public Event activeEvent;
	
    private static EventQueue instance;
    public static EventQueue Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EventQueue();
            }
            return instance;
        }
    }

    public bool IsBusy
    {
        get
        {
            if (activeEvent != null)
                return true;
            else
                return false;
        }
    }

    void Awake()
    {
        // First we check if there are any other instances conflicting
        if (instance != null && instance != this)
        {
            Debug.Log("There's already a Event Queue in the scene, destroying this one.");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            Debug.Log("Event Queue woke");
        }
    }

    // Use this for initialization
    void Start () {
        gameEvents = new Queue<Event>();

    }
	
	void Update () {

        //check if we need to do something
        if (activeEvent != null)
        {
            if (activeEvent.IsFinished)
            {
                activeEvent.EventEnd();
                activeEvent = null;
                gameEvents.Dequeue(); //remove last event
            }
            else
            {
                if (!activeEvent.IsStarted)
                    activeEvent.EventStart();

                activeEvent.Update();
            }
        }

        if (gameEvents.Count > 0)
        {
            activeEvent = gameEvents.Peek();
        }
    }

    public void ChangeScene(string levelName)
    {
        gameEvents.Enqueue(new ChangeSceneEvent(levelName));
    }

    public void AddAction(Action method)
    {
        ActionEvent ev = new ActionEvent(method);
        gameEvents.Enqueue(ev);
    }

    public void WaitABit(float duration)
    {
        gameEvents.Enqueue(new WaitEvent(duration));
    }
}
