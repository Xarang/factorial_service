using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FactorialService {
    class Service {

        static private ComputationCollection computationCollection = new ComputationCollection();

        /*
         *  return format
         */
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

        /*
         * Factorial Service called by the client
         */
        public TaskCompletionSource<FactorialResult> getFactorial(int n) {

            var promise = new TaskCompletionSource<FactorialResult>();
            //Task<FactorialResult> task = promise.Task;
            Task.Factory.StartNew(() => {
                
                //compute execution time
                Stopwatch watch = new Stopwatch();
                watch.Start();

                uint factorialValue = Service.computationCollection.GetFactorial(n);

                watch.Stop();
                promise.SetResult(new FactorialResult(factorialValue, watch.Elapsed));
            });
            return promise;
        }
        public string getResults()
        {
            return computationCollection.ResultsToJson();
        }

        public string Save()
        {
            return computationCollection.Save();
        }
    }
}
