using CrowGroup.Utility.Scriptable;
using System;
using UltEvents;
using UnityEngine;
using UnityEngine.Serialization;


namespace CrowSupport.Events
{
    [CreateAssetMenu(fileName = "GameEvent", menuName = "Scriptable Objects/Events/Game Event", order = -1005)]
    public class GameEvent : ScriptableObject
    {
        [FormerlySerializedAs("gameEvent")]
        [SerializeField]
        private UltEvent _response;
        private string _eventName => name;
        public string EventName => _eventName;

        public virtual void Invoke()
        {
            if (_response == null)
            {
                DebugLogger.LogError($"Response is null for {_eventName}");
                return;
            }

            if (!_response.HasCalls)
                DebugLogger.LogWarning($"Calling event \"{_eventName}\" but no listeners were found");

            //DebugLogger.Log($"Calling \"{_eventName}\"");

            _response.Invoke();
        }

        public bool HasListeners => _response != null;

        public void AddListener(Action method) => _response += method;

        public void RemoveListener(Action method) => _response -= method;
    }

    public class GameEvent<T> : ScriptableObject
    {
        [FormerlySerializedAs("gameEvent")]
        [SerializeField]
        private UltEvent<T> _response;
        private string _eventName => name;
        public string EventName => _eventName;

        public virtual void Invoke(T parameter)
        {
            if (_response == null)
            {
                DebugLogger.LogError($"Response is null for {_eventName}");
                return;
            }

            if (!_response.HasCalls)
                DebugLogger.LogWarning($"Calling event \"{_eventName}\" but no listeners were found");

            //DebugLogger.Log($"Calling \"{_eventName}\"");

            _response.Invoke(parameter);
        }

        public void Invoke(VariableSO<T> parameter)
        {
            Invoke(parameter.CurrentValue);
        }

        public bool HasListeners => _response != null;

        public void AddListener(Action<T> method) => _response += method;

        public void RemoveListener(Action<T> method) => _response -= method;
    }
}