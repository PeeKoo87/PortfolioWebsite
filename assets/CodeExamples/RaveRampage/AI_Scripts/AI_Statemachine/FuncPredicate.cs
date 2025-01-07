using System;

namespace Cyber_Slicer_AI
{
    public class FuncPredicate : IPredicate_Interface
    {
        readonly Func<bool> func;

        public FuncPredicate(Func<bool> func)
        {
            this.func = func;
        }

        public bool Evaluate() => func.Invoke();
       
    }
}