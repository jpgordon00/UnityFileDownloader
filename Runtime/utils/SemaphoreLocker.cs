using System;
using System.Threading;
using System.Threading.Tasks;

namespace UFD
{
    
    /// <summary>
    /// Used to handle async locking while being able to await
    /// </summary>
    public class SemaphoreLocker
    {
    protected readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
 
    public async Task LockAsync(Func<Task> worker, object args = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            await worker();
        }
        finally
        {
            _semaphore.Release();
        }
    }
 
    // overloading variant for non-void methods with return type (generic T)
    public async Task<T> LockAsync<T>(Func<Task<T>> worker, object args = null)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await worker();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

}
 