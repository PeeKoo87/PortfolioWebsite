namespace Cyber_Slicer_AI
{
    public interface ITransition_Interface { 

        IState_Interface To {  get; }
        IPredicate_Interface Condition { get; }
    }
}