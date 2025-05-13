using Plugins.DialogueSystem.Scripts.DialogueGraph.Attributes;
using UnityEngine;

namespace Plugins.DialogueSystem.Scripts.DialogueGraph.Nodes.TextContainers
{
    [EditorPath("TextContainers")]
    public class LocalizedTextContainer : TextContainer
    {
        [SerializeField] private string localePlayerPref;
        [SerializeField] private string defaultLocale;
        [SerializeField] private UDictionary<string, string> localizations = new();
        public override string Text
        {
            get
            {
                var locale = PlayerPrefs.HasKey(localePlayerPref)
                    ? PlayerPrefs.GetString(localePlayerPref)
                    : defaultLocale;
                if (localizations.TryGetValue(locale, out var text)) return text;
                return localizations.Values.Count > 0 ? localizations.Values[0] : "";
            }
        }

        public override AbstractNode Clone()
        {
            var clone = base.Clone() as LocalizedTextContainer;
            clone.localePlayerPref = localePlayerPref;
            clone.defaultLocale = defaultLocale;
            foreach (var pair in localizations) 
                clone.localizations[pair.Key] = pair.Value;
            return clone;
        }
    }
}