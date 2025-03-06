using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace StoryCreator.Editor.VisualElements
{
    public class Sidebar : VisualElement
    {
        private VisualElement _categories;
        private Label _titleLabel;

        private Dictionary<string, Category> _addedCategories = new Dictionary<string, Category>();

        public string title
        {
            get => _titleLabel.text;
            set => _titleLabel.text = value;
        }

        public VisualElement Content
        {
            get; private set;
        }

        public Sidebar()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("StoryCreator/Styles"));

            _titleLabel = new Label();
            _categories = new VisualElement();

            Content = new VisualElement();

            _titleLabel.AddToClassList("sidebar-title");
            Content.AddToClassList("sidebar-content");

            this.Add(_titleLabel);
            this.Add(_categories);
            this.Add(Content);
        }


        public Category AddCategory(string category)
        {
            if (!_addedCategories.TryGetValue(category, out Category cat))
            {
                _addedCategories[category] = new Category(category);
                _categories.Add(_addedCategories[category]);
            }
            return _addedCategories[category];
        }

        public void AddToCategory(string category, VisualElement element)
        {
            var cat = AddCategory(category);
            cat.Content.Add(element);
        }

        public void ClearCategories()
        {
            _addedCategories.Clear();
            _categories.Clear();
        }
    }
}