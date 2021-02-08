using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DestroyTime : MonoBehaviour
{
    [SerializeField] private bool _isPlayOnAwake = true;
    [SerializeField] private float _time;
    [SerializeField] private UnityEvent _destroy;

    private void Awake()
    {
        if (_isPlayOnAwake) StartDestroy();
    }

    public void StartDestroy()
    {
        StartCoroutine(ProcessDestroy(_time));
    }

    private IEnumerator ProcessDestroy(float time)
    {
        yield return new WaitForSeconds(time);

        Destroy(gameObject);
        _destroy.Invoke();
    }
}
