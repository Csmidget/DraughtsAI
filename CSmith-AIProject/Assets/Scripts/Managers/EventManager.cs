using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager {

    static Dictionary<string, UnityAction> actionDict;

	public static void Init()
    {
        actionDict = new Dictionary<string, UnityAction>();
    }

    public static bool TriggerEvent(string _eventName)
    {
        actionDict[_eventName]();
        return true;
    }

    public static bool CreateEvent(string _eventName, UnityAction _action)
    {

        if (actionDict.ContainsKey(_eventName))
        {
            Debug.LogError("Attempted to create event that already exists: " + _eventName);
            return false;
        }

        actionDict.Add(_eventName, _action);

        return true;
    }

    public static bool RegisterToEvent(string _eventName, UnityAction _action)
    {

        if (!actionDict.ContainsKey(_eventName))
        {
            Debug.LogError("Attempted to register for non-existent event: " + _eventName);
            return false;
        }

        actionDict[_eventName] += _action;
        return true;
    }

    public static bool UnRegisterFromEvent(string _eventName, UnityAction _action)
    {
        if (!actionDict.ContainsKey(_eventName))
            actionDict[_eventName] -= _action;

        return true;
    }


}
