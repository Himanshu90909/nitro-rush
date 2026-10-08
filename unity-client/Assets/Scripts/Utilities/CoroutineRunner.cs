using UnityEngine;

namespace NitroRush.Utilities
{
    /// <summary>
    /// Static MonoBehaviour host allowing non-MonoBehaviour classes to execute Coroutines safely.
    /// </summary>
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        public static CoroutineRunner Instance
        {
            get {
                if (_instance == null)
                {
                    var obj = new GameObject("[CoroutineRunner]");
                    _instance = obj.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }
    }
}
