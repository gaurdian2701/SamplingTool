using System.Threading;
using UnityEngine;
using System.Threading.Tasks;

public class TaskCancellationTest : MonoBehaviour
{
    private CancellationTokenSource _mainCancellationTokenSource;
    private CancellationToken _cancellationToken;
    private void Start()
    {
        _mainCancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _mainCancellationTokenSource.Token;
    }   
    
    private async void CancellableTask()
    {
        for (int i = 0; i < 10000; i++)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                Debug.Log("Cancellation requested");
                break;
            }
            Debug.Log("Task Running");
            await Task.Delay(10);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            _mainCancellationTokenSource.Cancel();
    }
}
