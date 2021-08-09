using System;
using System.Threading.Tasks;

namespace FactorialService {
    class Service {

        static private ComputationCollection computationCollection = new ComputationCollection();
        
        public uint getFactorial(int n) {
    
            Task<uint> newTask = Task<uint>.Factory.StartNew(() => Service.computationCollection.getFactorial(n));
            //TODO: store this task somewhere
            return newTask.Result;
        }
        
    }
}
