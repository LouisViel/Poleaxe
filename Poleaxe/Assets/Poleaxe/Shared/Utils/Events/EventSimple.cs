using System.Collections.Generic;
using System.Linq;

namespace Poleaxe.Utils.Event
{
    public class PoleaxeEvent<key>
    {
        public PoleaxeEvent() { }
        private List<PoleaxeEventData<key>> invocationList = new List<PoleaxeEventData<key>>();

        public void Invoke()
        {
            RemoveNull();
            if (invocationList.Count > 0) {
                PoleaxeEventData<key> invocation = invocationList.Last();
                ((EventHandler)invocation.Delegate).Invoke();
                if (invocation.RemoveOnCall) invocationList.Remove(invocation);
            }
        }

        public void Register(key key, EventHandler @delegate) => Register(key, @delegate, false);
        public void Register(key key, EventHandler @delegate, bool removeOnCall) => Register(key, @delegate, removeOnCall, false);
        public void Register(key key, EventHandler @delegate, bool removeOnCall, bool freeze)
        {
            invocationList.Add(new PoleaxeEventData<key> {
                RemoveOnCall = removeOnCall, Freeze = freeze,
                Key = key, Delegate = @delegate
            });
        }

        public void Remove(EventHandler @delegate)
        {
            RemoveNull();
            invocationList.RemoveAll((PoleaxeEventData<key> invocation) => {
                return invocation.Delegate.Equals(@delegate);
            });
        }

        public void Freeze(EventHandler @delegate, bool freeze)
        {
            RemoveNull();
            for (int i = 0; i < invocationList.Count; ++i) {
                PoleaxeEventData<key> invocation = invocationList[i];
                if ((EventHandler)invocation.Delegate == @delegate) {
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