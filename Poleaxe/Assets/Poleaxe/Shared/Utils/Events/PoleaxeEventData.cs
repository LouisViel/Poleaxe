using System;

namespace Poleaxe.Utils.Event
{
    internal struct PoleaxeEventData<key>
    {
        public key Key;
        public Delegate Delegate;
        public bool RemoveOnCall;
        public bool Freeze;
    }
}