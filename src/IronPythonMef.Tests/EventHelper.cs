using System;
using System.Threading;

namespace IronPythonMef.Tests
{
    public static class EventHelper
    {
        public static void WaitForEvent(Action<EventHandler> addEvent, Action<EventHandler> removeEvent, Action action)
        {
            var readyFlag = new AutoResetEvent(false);
            EventHandler eventHandler = (e, s) => readyFlag.Set();
            addEvent(eventHandler);
            action();
            if (!readyFlag.WaitOne(TimeSpan.FromSeconds(10)))
            {
                throw new InvalidOperationException("Event wasn't raised!");
            }
            ;
            removeEvent(eventHandler);
        }
    }
}
