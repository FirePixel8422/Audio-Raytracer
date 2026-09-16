using System;
using UltEvents;
using UnityEngine;

namespace CrowSupport.Events
{
    [Serializable]
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField]
        protected GameEventSO _event;

        [SerializeField]
        protected UltEvent _response;

        public void AddResponse(Action response) => _response.AddPersistentCall(response);
        public void RemoveResponse(Action response) => _response.RemovePersistentCall(response);


        protected virtual void OnEnable()
        {
            if (_event == null)
            {
                DebugLogger.LogError($"Event listener on \"{name}\" does not have a event to listen to selected");
                return;
            }
            _event.AddListener(OnEvent);
        }

        protected virtual void OnDisable()
        {
            if (_event == null)
            {
                DebugLogger.LogError($"Event listener \"{name}\" does not have a event to listen to selected");
                return;
            }
            _event.RemoveListener(OnEvent);
        }

        public virtual void OnEvent()
        {
            if (_response == null)
            {
                DebugLogger.LogError($"Event listener \"{name}\" does not have a response to call when an event is fired");
                return;
            }
            _response.Invoke();
        }
    }

    [Serializable]
    public class GameEventListener<T> : MonoBehaviour
    {
        [SerializeField]
        protected GameEventSO<T> _event;

        [SerializeField]
        protected UltEvent<T> _response;

        public void AddResponse(Action<T> response) => _response.AddPersistentCall(response);
        public void RemoveResponse(Action<T> response) => _response.RemovePersistentCall(response);


        protected virtual void OnEnable()
        {
            if (_event == null)
            {
                DebugLogger.LogError($"Event listener on \"{name}\" does not have a event to listen to selected");
                return;
            }
            _event.AddListener(OnEvent);
        }

        protected virtual void OnDisable()
        {
            if (_event == null)
            {
                DebugLogger.LogError($"Event listener \"{name}\" does not have a event to listen to selected");
                return;
            }
            _event.RemoveListener(OnEvent);
        }

        public virtual void OnEvent(T value)
        {
            if (_response == null)
            {
                DebugLogger.LogError($"Event listener \"{name}\" does not have a response to call when an event is fired");
                return;
            }
            _response.Invoke(value);
        }
    }
}