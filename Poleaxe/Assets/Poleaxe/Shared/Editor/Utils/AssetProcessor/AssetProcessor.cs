using System.Collections.Generic;
using Poleaxe.Utils.Event;

namespace Poleaxe.Editor.Utils.AssetProcessor
{
    public static class AssetProcessor
    {
        internal static List<string> AboutBeingCreated = new List<string>();
        internal static List<string> AboutBeingSaved = new List<string>();

        public static PoleaxeEventMultiple<object, AssetProcessing> OnCreate { get; } = new PoleaxeEventMultiple<object, AssetProcessing>();
        public static PoleaxeEventMultiple<object, AssetProcessing> OnDelete { get; } = new PoleaxeEventMultiple<object, AssetProcessing>();
        public static PoleaxeEventMultiple<object, AssetProcessing> OnMove { get; } = new PoleaxeEventMultiple<object, AssetProcessing>();
        public static PoleaxeEventMultiple<object, AssetProcessing> OnSave { get; } = new PoleaxeEventMultiple<object, AssetProcessing>();

        public static void RegisterToAll(EventHandler<AssetProcessing> @method)
        {
            OnCreate.Register(null, @method);
            OnDelete.Register(null, @method);
            OnMove.Register(null, @method);
            OnSave.Register(null, @method);
        }

        public static void RemoveFromAll(EventHandler<AssetProcessing> @method)
        {
            OnCreate.Remove(@method);
            OnDelete.Remove(@method);
            OnMove.Remove(@method);
            OnSave.Remove(@method);
        }
    }
}