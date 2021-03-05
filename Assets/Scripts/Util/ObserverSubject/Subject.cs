using UnityEngine;
using System.Collections.Generic;

namespace Util.ObserverSubject
{
    public abstract class Subject : MonoBehaviour
    {
        private List<Observer> _observers = new List<Observer>();

        public void RegisterObserver(Observer observer)
        {
            _observers.Add(observer);
        }

        public void Notify(object value, NotificationType notificationType)
        {
            foreach(var observer in _observers)
            {
                observer.OnNotify(value, notificationType);
            }
        }
    }
}

