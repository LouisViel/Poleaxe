using System;
using System.Collections.Generic;

namespace Poleaxe.Utils.Event
{
    public class PoleaxeEventMultiple<key, data>
    {
        public PoleaxeEventMultiple() { }
        private List<PoleaxeEventData<key>> invocationList = new List<PoleaxeEventData<key>>();
        private event EventHandler<data> EventHandler;

        public void Invoke(data data)
        {
            RemoveNull();
            EventData<data> eventData = new EventData<data>(data);
            EventHandler?.Invoke(null, eventData);
            RemoveOnCall();
        }

        public void Register(key key, EventHandler<data> @delegate) => Register(key, @delegate, false);
        public void Register(key key, EventHandler<data> @delegate, bool removeOnCall) => Register(key, @delegate, removeOnCall, false);
        public void Register(key key, EventHandler<data> @delegate, bool removeOnCall, bool freeze)
        {
            EventHandler += @delegate;
            invocationList.Add(new PoleaxeEventData<key> {
                RemoveOnCall = removeOnCall, Freeze = freeze,
                Key = key, Delegate = @delegate
            });
        }

        public void Remove(EventHandler<data> @delegate)
        {
            RemoveNull();
            invocationList.RemoveAll((PoleaxeEventData<key> invocation) => {
                if (invocation.Delegate.Equals(@delegate)) {
                    EventHandler -= (EventHandler<data>)invocation.Delegate;
                    return true;
                }
                return false;
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
                if (!invocation.Freeze && invocation.Key.Equals(key)) {
                    EventHandler -= (EventHandler<data>)invocation.Delegate;
                    return true;
                }
                return false;
            });
        }

        public void Clear()
        {
            invocationList.Clear();
            if (EventHandler != null) {
                foreach (Delegate @delegate in EventHandler.GetInvocationList()) {
                    EventHandler -= (EventHandler<data>)@delegate;
                }
            }
        }

        private void RemoveNull()
        {
            invocationList.RemoveAll((PoleaxeEventData<key> invocation) => {
                if (invocation.Delegate == null) {
                    EventHandler -= (EventHandler<data>)invocation.Delegate;
                    return true;
                }
                return false;
            });
        }

        private void RemoveOnCall()
        {
            List<PoleaxeEventData<key>> toRemove = new List<PoleaxeEventData<key>>();
            foreach (PoleaxeEventData<key> invocation in invocationList) {
                if (!invocation.RemoveOnCall) continue;
                EventHandler -= (EventHandler<data>)invocation.Delegate;
                toRemove.Add(invocation);
            }
            foreach (PoleaxeEventData<key> invocation in toRemove) {
                invocationList.Remove(invocation);
            }
        }
    }
}