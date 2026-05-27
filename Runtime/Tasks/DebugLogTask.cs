using Moths.Serialization;
using UnityEngine;

namespace Moths.Stories.Tasks
{
    [System.Serializable]
    [InterfaceReference("Debug/Log")]
    public class DebugLogTask : ITask
    {
        [SerializeField] string _text;
        public void Do()
        {
            Debug.Log(_text);
        }
    }
}