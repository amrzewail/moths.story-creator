using Moths.Serialization;
using UnityEngine;

namespace Moths.Stories.Tasks
{
    [System.Serializable]
    [InterfaceReference("Debug/Log")]
    public class DebugLogTask : ITask
    {
        public string Description => $"Log {_text}";

        [SerializeField] string _text;
        public void Execute()
        {
            Debug.Log(_text);
        }
    }
}