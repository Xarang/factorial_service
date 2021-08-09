/*
*  This is the class that is meant to store the results accumulated by the service while running
*  It must provide a performance boost to the service assuming it runs 24/7
*  It needs to have a way to be stored into a file and loaded back if the service was to be stopped for a while
*/

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using System.Threading.Tasks;

namespace FactorialService {
    class ComputationCollection {
        private static uint[] results = new uint[20];
        private static ConcurrentQueue<int> computeQueue = new ConcurrentQueue<int>();
        private Task queueReader;

        private static Action performComputation = () => {
            results[0] = 1; //TODO: do this only once elsewhere
            while (true) {
                int n;
                if (computeQueue.TryDequeue(out n)) {
                    if (results[n] != 0) {
                        return;
                    } else {
                        uint mult = 1;
                        uint n_decr = (uint)n;
                        while (n_decr > 0 && results[n_decr] == 0) {
                            mult *= n_decr;
                            n_decr--;
                        }
                        results[n] = mult * results[n_decr];
                    }
                }
            }
        };

        public ComputationCollection() { //TODO: optionally take file into input
            this.queueReader = Task.Factory.StartNew(performComputation);
        }

        ~ComputationCollection() {
            this.queueReader.Wait();
        }

        public uint getFactorial(int n) {
            if  (0 > n || n > 20) {
                return 0;
            }
            if (results[n] == 0) {
                computeQueue.Enqueue(n);
                Thread.Sleep(50);
                return getFactorial(n);
            } else {
                return results[n];
            }
        }

    }
}
