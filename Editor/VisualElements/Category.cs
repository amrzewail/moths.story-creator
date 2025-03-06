using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace StoryCreator.Editor.VisualElements
{
    public class Category : VisualElement
    {
        public VisualElement Content { get; private set; }

        public Category(string title) 
        {
            styleSheets.Add(Resources.Load<StyleSheet>("StoryCreator/Styles"));

            Label label = new Label();
            label.text = title;

            this.Add(label);

            Content = new VisualElement();
            Content.AddToClassList("content");
            this.Add(Content);
        }
    }
}