using System;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorPath : Attribute
    {
        public readonly string path;

        public EditorPath(string path)
        {
            this.path = path.Replace("\\", "/");
        }
    }
}