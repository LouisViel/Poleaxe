using System.Collections.Generic;
using System.Linq;

namespace Poleaxe
{
    public class LifetimeStack
    {
        private Dictionary<string, float> stack = new Dictionary<string, float>();

        public bool Register(string key, float lifetime)
        {
            if (stack.ContainsKey(key)) return false;
            stack.Add(key, lifetime);
            return true;
        }

        public void Update(float time)
        {
            List<string> toRemove = new List<string>();
            string[] keys = stack.Keys.ToArray();

            foreach (string key in keys) {
                float lifetime = stack[key] - time;
                if (lifetime <= 0f) toRemove.Add(key);
                else stack[key] = lifetime;
            }

            foreach (string key in toRemove) {
                stack.Remove(key);
            }
        }
    }
}