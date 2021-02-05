using UnityEngine;

namespace Differences
{
    public class PullObjectsUI : MonoBehaviour
    {
        [SerializeField] private int pullCount;
        [SerializeField] private GameObject pullObject;
        [SerializeField] private Transform targetInstantiate;

        /// <summary>
        /// StartPull system method
        /// </summary>
        /// <typeparam name="T">Must be inheritance with MonoBehaviour</typeparam>
        /// <returns></returns>
        internal T[] StartPull<T>()
        {
            var cacheElement = Instantiate(pullObject, targetInstantiate);
            cacheElement.name = pullObject.name;

            var array = new T[pullCount];

            for (var i = 0; i < pullCount; i++)
            {
                var element = Instantiate(cacheElement, targetInstantiate);
                element.name = cacheElement.name;
                array[i] = element.GetComponent<T>();
            }

            cacheElement.gameObject.SetActive(false);

            return array;
        }
    }
}
