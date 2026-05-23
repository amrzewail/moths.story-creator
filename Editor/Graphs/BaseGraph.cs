using Moths.Graphs.Editor;

namespace Moths.Stories.Editor.Graphs
{
    public abstract class BaseGraph<TData> : BasicGraphVisualElement
    {

        public abstract void Initialize(StoryCreator editor, TData data);

    }
}