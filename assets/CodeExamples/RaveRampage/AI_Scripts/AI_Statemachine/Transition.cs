namespace Cyber_Slicer_AI
{
    public class Transition : ITransition_Interface
    {
        public IState_Interface To { get; }
        public IPredicate_Interface Condition { get; }

        public Transition(IState_Interface to, IPredicate_Interface condition)
        {
            To = to;
            Condition = condition;
        }
    }
}