using UnityEngine;

namespace Moths.Stories.Tasks
{
    [System.Serializable]
    public class DebugLogTask : ITask
    {
        [SerializeField] string _text;
        public void Do()
        {
            Debug.Log(_text);
        }
    }
}