using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager {

    static Dictionary<string, UnityAction> eventDict;

	public static void Init()
    {
        eventDict = new Dictionary<string, UnityAction>();
    }

    public static bool TriggerEvent(string _eventName)
    {
        if (!eventDict.ContainsKey(_eventName))
        {
            Debug.LogError("Attempted to trigger event that does not exist: " + _eventName);
            return false;
        }

        if (eventDict[_eventName] != null)
        {
            eventDict[_eventName].Invoke();
        }

        return true;
    }

    public static bool CreateEvent(string _eventName)
    {
        UnityAction action = null;


        if (eventDict.ContainsKey(_eventName))
        {
            Debug.Log("Attempted to create event that already exists: " + _eventName);
            return true;
        }

        eventDict.Add(_eventName, action);

        return true;
    }

    public static bool RegisterToEvent(string _eventName, UnityAction _action)
    {

        if (!eventDict.ContainsKey(_eventName))
        {
            Debug.Log("Attempted to register for non-existent event: " + _eventName + ". Event created.");
            CreateEvent(_eventName);
        }

        eventDict[_eventName] += _action;
        return true;
    }

    public static bool UnRegisterFromEvent(string _eventName, UnityAction _action)
    {
        if (!eventDict.ContainsKey(_eventName))
            eventDict[_eventName] -= _action;

        return true;
    }


}
