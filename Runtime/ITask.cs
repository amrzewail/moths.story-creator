namespace Moths.Stories
{
    public interface ITask
    {
        string Description { get; }
        void Execute();
    }
}