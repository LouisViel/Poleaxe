using System;

namespace Poleaxe
{
    internal class SingletonException : Exception
    {
        public SingletonException() { }
        public SingletonException(string message) : base(message) { }
        public SingletonException(string message, Exception innerException) : base(message, innerException) { }
    }

    public static class Singleton<t>
    {
        private static t singleton;
        public static t Get() => singleton;
        public static void Set(t value) => Set(value, true);
        public static void Set(t value, bool onlyOne)
        {
            if (onlyOne && singleton != null) {
                string error = $"There is already a {typeof(t)} Singleton";
                throw new SingletonException(error);
            } else singleton = value;
        }
    }
}