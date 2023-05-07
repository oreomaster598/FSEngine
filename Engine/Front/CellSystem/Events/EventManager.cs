using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.Front.CellSystem.Events
{
    public delegate void EventHandler(Event sender);
    public class Event
    {
        /// <summary>
        /// The number of times the event was triggered (ignoring callonce).
        /// </summary>
        public int triggeredCount;
        internal bool triggered;

        /// <summary>
        /// Call only once per frame.
        /// Does not effect triggeredCount it only effects the callback.
        /// </summary>
        public bool callonce;
        public EventHandler callback;

        public Event(bool callonce, EventHandler callback)
        {
            this.callonce = callonce;
            this.callback = callback;
        }

        public void Trigger()
        {
            if(!(callonce && triggered))
            {
                triggeredCount++;
                triggered = true;
                callback(this);
            }
        }
    }
    public static class EventManager
    {
        private static Dictionary<int, Event> events = new Dictionary<int, Event>();
        private static List<int> eof_triggers = new List<int>();
        static int avail_id = 0;
        public static int RegisterEvent(Event e)
        {
            avail_id++;
            events.Add(avail_id, e);
            return avail_id;
        }
        public static void UnregisterEvent(int id)
        {
            events.Remove(id);
        }
        /// <summary>
        /// Immediately trigger an event.
        /// </summary>
        /// <param name="id"></param>
        public static void TriggerEvent(int id)
        {
            events[id].Trigger();
        }
        /// <summary>
        /// Queue an event to be triggered at the end of the frame.
        /// </summary>
        /// <param name="id"></param>
        public static void QueueEvent(int id)
        {
            events[id].triggeredCount++;
            if(!(eof_triggers.Contains(id) && events[id].callonce))
                eof_triggers.Add(id);
        }
        public static void FlushQueue()
        {
            for (int i = 0; i < eof_triggers.Count; i++)
            {
                events[eof_triggers[i]].Trigger();
            }
            eof_triggers.Clear();
        }
        public static void ResetTriggers()
        {
            foreach (Event e in events.Values)
            {
                e.triggered = false;
                e.triggeredCount = 0;
            }
        }
    }
}
