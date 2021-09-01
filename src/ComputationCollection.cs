/*
*  This is the class that is meant to store the results accumulated by the service while running
*  It must provide a performance boost to the service assuming it runs 24/7
*  It needs to have a way to be stored into a file and loaded back if the service was to be stopped for a while
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FactorialService {
    class ComputationCollection {
        private static uint[] results = new uint[20];
        private static ConcurrentQueue<int> computeQueue = new ConcurrentQueue<int>();
        private static string resultFilename = "results.json";
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
            Load();
            this.queueReader = Task.Factory.StartNew(performComputation);
        }

        public void Terminate() {
            this.queueReader.Wait();
            Save();
        }

        public static string ResultsAsString() {
            string str = "";
            foreach (var indexedValue in results.Select((x, index) => new { x, index }))
            {
                var value = indexedValue.x;
                var index = indexedValue.index;
                str += $"[{index}]: {(value != 0 ? value : '-')}\n";
            }
            return str;
        }

        /*
         * Save result array in a file as a JSON array
         */
        public async void Save() {
            Debug.WriteLine($"Outputting following values to result file:");
            Debug.WriteLine(ResultsAsString());
            await File.WriteAllTextAsync(resultFilename, JsonSerializer.Serialize(results));
        }

        public void Load() {
            if (!File.Exists(resultFilename)) {
                Debug.WriteLine($"Could not find result file '{resultFilename}'. Will resume with empty array");
                return;
            }
            string text = File.ReadAllText(resultFilename);
            results = JsonSerializer.Deserialize<uint[]>(resultFilename);
            Debug.WriteLine("Loaded following values from result file:");
            Debug.WriteLine(ResultsAsString());
        }

        public uint GetFactorial(int n) {
            if  (0 > n || n > 20) {
                return 0;
            }
            if (results[n] == 0) {
                computeQueue.Enqueue(n);
                Thread.Sleep(50);
                return GetFactorial(n);
            } else {
                return results[n];
            }
        }

    }
}
