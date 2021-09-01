using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FactorialService {
    class Service {

        static private ComputationCollection computationCollection = new ComputationCollection();

        public void Terminate() {
            computationCollection.Terminate();
        }

        public struct FactorialResult {
            public uint result;
            public TimeSpan executionTime;
            public FactorialResult(uint result, TimeSpan executionTime) {
                this.result = result;
                this.executionTime = executionTime;
            }
            public string ToJson() {
                return "{ "
                    + "result: "
                    + this.result
                    + ", "
                    + "execution time: "
                    + this.executionTime
                    + " }";
            }
        }

        public TaskCompletionSource<FactorialResult> getFactorial(int n) {

            var promise = new TaskCompletionSource<FactorialResult>();
            //Task<FactorialResult> task = promise.Task;
            Task.Factory.StartNew(() => {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                Task<uint> newTask = Task<uint>.Factory.StartNew(() => Service.computationCollection.GetFactorial(n));
                watch.Stop();
                //TODO: store this task somewhere
                promise.SetResult(new FactorialResult(newTask.Result, watch.Elapsed));
            });
            return promise;
        }
    }
}
