using System;

namespace Moths.StoryCreator.Attributes
{
    public class QuestElementInfoAttribute : Attribute
    {
        public string Category { get; private set; }
        public string Name { get; private set; }
        public string Icon { get; private set; }
        public QuestElementInfoAttribute(string category, string name, string icon = "") 
        {
            Name = name;
            Category = category;
            Icon = icon;
        }
    }
}