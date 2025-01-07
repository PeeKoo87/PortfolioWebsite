
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Cyber_Slicer_AI
{
    public class StateMachine {
        StateNode current;
        Dictionary<Type, StateNode> nodes = new();
        HashSet<ITransition_Interface> anyTransitions = new();

        public void Update()
        {
            var transition = GetTransition();
            if (transition != null) 
                ChangeState(transition.To);
            
            current.State?.Update();
        }

        public void FixedUpdate()
        {
            current.State?.FixedUpdate();
        }

        public void SetState(IState_Interface state) {
            current = nodes[state.GetType()];
            current.State?.OnEnter();
        }

        void ChangeState(IState_Interface state)
        {
            if (state == current.State) return;

            var previousState = current.State;
            var nextState = nodes[state.GetType()].State;

            previousState?.OnExit();
            nextState?.OnEnter();
            current = nodes[state.GetType()];
        }

        ITransition_Interface GetTransition()
        {

            //foreach (var transition in current.Transitions)
            //{
            //    if (transition.Condition.Evaluate()) { return transition; }
            //}
            //return null;

            //Jotain häikkää loopeissa
            foreach (var transition in anyTransitions)
            {
                if (transition.Condition.Evaluate())
                {
                    return transition;
                }
            }

            foreach (var transition in current.Transitions)
            {

                if (transition.Condition.Evaluate())
                {
                    return transition;
                }

            }
            return null;
        }

        public void AddTransition(IState_Interface from, IState_Interface to, IPredicate_Interface condition)
        {
            GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
        }

        public void AddAnyTransition(IState_Interface to, IPredicate_Interface condition)
        {
            anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));
        }

        StateNode GetOrAddNode(IState_Interface state) {
            var node = nodes.GetValueOrDefault(state.GetType());

            if(node == null)
            {
                node = new StateNode(state);
                nodes.Add(state.GetType(), node);
            }
            return node;
        }

        class StateNode
        {
            public IState_Interface State { get; }

            public HashSet<ITransition_Interface> Transitions { get; }

            public StateNode(IState_Interface state)
            {
                State = state;
                Transitions = new HashSet<ITransition_Interface>();
            }

            public void AddTransition(IState_Interface to, IPredicate_Interface condition)
            {
                Transitions.Add(new Transition(to, condition));
            }
        }
    }
}