using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace StoryCreator.Editor.VisualElements
{
    public class HistoryStack : VisualElement
    {
        private List<Button> _buttons;

        public HistoryStack() 
        {
            styleSheets.Add(Resources.Load<StyleSheet>("StoryCreator/Styles"));

            _buttons = new();
        }

        public void Refresh()
        {
            Clear();
            for (int i = 0; i < _buttons.Count; i++)
            {
                Button btn = _buttons[i];
                Add(btn);
                if (i < _buttons.Count - 1) Add(new Label(">"));
            }
        }

        public void AddToStack(string name, Action clickCallback)
        {
            int stackCount = _buttons.Count;
            _buttons.Add(new Button(() =>
            {
                StackButtonClickCallback(stackCount, clickCallback);
            })
            {
                text = name,
            });

            Refresh();
        }

        private void StackButtonClickCallback(int index, Action clickCallback)
        {
            clickCallback?.Invoke();
            while(_buttons.Count != index + 1)
            {
                _buttons.RemoveAt(_buttons.Count - 1);
            }
            Refresh();
        }
    }
}