using UnityEngine;

namespace Util.ObserverSubject
{
    public abstract class Observer : MonoBehaviour
    {
        public abstract void OnNotify(object value, NotificationType notificationType);
    }
}

