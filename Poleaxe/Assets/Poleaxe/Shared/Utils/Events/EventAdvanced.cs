using System.Collections.Generic;
using System.Linq;

namespace Poleaxe.Utils.Event
{
    public class PoleaxeEvent<key, data>
    {
        public PoleaxeEvent() { }
        private List<PoleaxeEventData<key>> invocationList = new List<PoleaxeEventData<key>>();

        public void Invoke(data data)
        {
            RemoveNull();
            if (invocationList.Count > 0) {
                EventData<data> eventData = new EventData<data>(data);
                PoleaxeEventData<key> invocation = invocationList.Last();
                ((EventHandler<data>)invocation.Delegate).Invoke(null, eventData);
                if (invocation.RemoveOnCall) invocationList.Remove(invocation);
            }
        }

        public void Register(key key, EventHandler<data> @delegate) => Register(key, @delegate, false);
        public void Register(key key, EventHandler<data> @delegate, bool removeOnCall) => Register(key, @delegate, removeOnCall, false);
        public void Register(key key, EventHandler<data> @delegate, bool removeOnCall, bool freeze)
        {
            invocationList.Add(new PoleaxeEventData<key> {
                RemoveOnCall = removeOnCall, Freeze = freeze,
                Key = key, Delegate = @delegate
            });
        }

        public void Remove(EventHandler<data> @delegate)
        {
            RemoveNull();
            invocationList.RemoveAll((PoleaxeEventData<key> invocation) => {
                return (EventHandler<data>)invocation.Delegate == @delegate;
            });
        }

        public void Freeze(EventHandler<data> @delegate, bool freeze)
        {
            RemoveNull();
            for (int i = 0; i < invocationList.Count; ++i) {
                PoleaxeEventData<key> invocation = invocationList[i];
                if ((EventHandler<data>)invocation.Delegate == @delegate) {
                    invocation.Freeze = freeze;
                }
            }
        }

        public void Clean(key key)
        {
            RemoveNull();
            invocationList.RemoveAll((PoleaxeEventData<key> invocation) => {
                return !invocation.Freeze && invocation.Key.Equals(key);
            });
        }

        public void Clear()
        {
            invocationList.Clear();
        }

        private void RemoveNull()
        {
            invocationList.RemoveAll((PoleaxeEventData<key> invocation) => {
                return invocation.Delegate == null;
            });
        }
    }
}