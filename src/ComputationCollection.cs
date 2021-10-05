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

    //this class holds the collections used for our factorial computation and storage
    class ComputationCollection {

        private static uint[] results = new uint[20]
        {
            1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };
        private static ConcurrentQueue<int> computeQueue = new ConcurrentQueue<int>();
        private static string resultFilename = "results.json";
        private Task queueReader;

        

        public ComputationCollection() {
            Load();
            this.queueReader = Task.Factory.StartNew(() =>
            {
                /*
                 * Loop running on the compute queue, checking if a new factorial value has to be computed
                 * and performing this computation if needed to compute fact(n), we use fact(x) with x the
                 * highest value for which fact has been computed so far
                */
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
        }


        /*
         *  Serializes the result collection to a json file (JSON serializer I found in .NET libs would not work for some reason, did not want to lose time on that)
         */
        public string ResultsToJson()
        {
            string str = "";
            foreach (var indexedValue in results.Select((x, index) => new { x, index }))
            {
                var value = indexedValue.x;
                var index = indexedValue.index;
                str += "   " + value;
                str += (index < results.Length - 1)  ? ",\n" : "\n";
            }
            return "[\n"
                + str
                + "]";
        }

        /*
         * Save result array in a file as a JSON array
         */
        public string Save()
        {
            try
            {
                lock(results)
                {
                    File.WriteAllTextAsync(resultFilename, ResultsToJson());
                    return "{\n"
                        + "message: \"Saved values to result file: " + resultFilename + "\",\n"
                        + "values: " + ResultsToJson() + "\n"
                        + "}";
                }
            }
            catch (Exception E)
            {
                return "{\n"
                    + "\"message\": \"was unable to save file at this location\",\n"
                    + $"\"filename\": \"{resultFilename}\",\n"
                    + $"\"error\": {E.ToString()}\"\n"
                    + "}\n";
            }

        }

        /*
         * loads previous results from a .json file
         */
        public void Load()
        {
            if (!File.Exists(resultFilename))
            {
                Debug.WriteLine($"Could not find result file '{resultFilename}'. Will resume with empty array");
                return;
            }
            else
            {
                Debug.WriteLine($"loading cached values in '{resultFilename}'");
                string text = File.ReadAllText(resultFilename);
                try
                {
                    results = JsonSerializer.Deserialize<uint[]>(text);
                    Debug.WriteLine("Loaded following values from result file:");
                    Debug.WriteLine(ResultsToJson());
                }
                catch(Exception e)
                {
                    Debug.WriteLine("unable to load cached values");
                    Debug.WriteLine(e.ToString());
                }
            }
        }


        /*
         * The function the client calls to compute a factorial;
         * If the value was not computed before, ask the compute queue to compute it before
         * Otherwise, simply return the result
         */
        public uint GetFactorial(int n)
        {
            if  (0 > n || n >= 20)
            {
                return 0;
            }
            if (results[n] == 0)
            {
                computeQueue.Enqueue(n);
                while (results[n] == 0)
                {
                    Thread.Sleep(50);
                }
            }
            return results[n];
        }

    }
}
