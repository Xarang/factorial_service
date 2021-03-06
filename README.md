# factorial_service

The aim of this project is to build a service that computes factorial values when requested by a client, using c# multithreading capabilities to help with performance.

## Communicating with the service

The service itself is a ASP.NET application with the following routes:

```
GET {{host}}/:int
0 <= int < 20
returns fact(int) with this format: { "result": uint, "execution time": TimeSpan }
```

```
GET {{host}}/results
returns a JSON array containing all fact values computed so far (or 0 for values that have yet to be computed once)
```


```
GET {{host}}/save
save all computed values in a JSON file that will be loaded on next service boot
```


## Inner Workings

Our service is divided in 2 main collections:
* a basic array of 20 _uint_ . Above fact(20) the numbers are too big to be represented anyway without bignums (out of scope). This array is **not thread-safe** and will only be **read** by the client (no thread-unsafe write operations).
* a **thread-safe** queue that holds fact compute to be performed. When a client requests a factorial value that has yet to be computed, this value is added to this queue. A task is running on this queue, dequeuing compute requests as they come, performing the compute, and storing the result into the array.

Most requests will directly access the result array, hence not using a thread-safe collection there is a big efficiency gain. The thread-safe compute queue on the other hand helps making sure there is no concurrent write happening on our array _(I used the postulate that whilst concurrent writes are bad, concurrent reads are ok)_

I used Task to do multithreading; the 2 types of tasks that this project instantiate are the following:
* a Task that handles client request
```
    //FactorialService.cs
    public TaskCompletionSource<FactorialDTO> getFactorial(int n) {
        var promise = new TaskCompletionSource<FactorialDTO>();
        Task.Factory.StartNew(() => {
            [...]
            uint factorialValue = Service.computationCollection.GetFactorial(n);
            [...]
            promise.SetResult(new FactorialDTO(factorialValue, watch.Elapsed));
        });
        return promise;
```
* a Task that loops on our ComputeQueue, waiting for a client request to compute a factorial value that has yet to be computed
```
    //ComputationCollection.cs
    this.queueReader = Task.Factory.StartNew(() =>
    {
        int n;
        while (true)
        {
            if (computeQueue.TryDequeue(out n))
            {
                if (results[n] != 0)
                {
                    return;
                }
                else
                {
                    uint mult = 1;
                    uint n_decr = (uint)n;
                    while (n_decr > 0 && results[n_decr] == 0)
                    {
                        mult *= n_decr;
                        n_decr--;
                    }
                    results[n] = mult * results[n_decr];
                }
            }
        }
    });
```
## Some thoughts on the project

I think the project would have been a better showcase of useful multithreading if we were using another function to compute, such as Fibonacci or Racaman's sequence. Factorial overflowing after 20 values means the actual computation has to be done only 20 times (but this is where multithreading is the most benificial)
