using System;
using System.Collections.Generic;

namespace TransitionStateMachine.EnumStates
{
    public abstract class BaseTransition<TStateType, TStateDataType> where TStateType : Enum
    {
        public HashSet<TStateType> FromStates { get; private set; }
        public TStateType ToState { get; private set; }

        public BaseTransition(TStateType from, TStateType to)
        {
            FromStates = new HashSet<TStateType>() { from };
            ToState = to;
        }

        public BaseTransition(HashSet<TStateType> fromStates, TStateType to)
        {
            FromStates = fromStates;
            ToState = to;
        }

        public virtual int GetTransitionOrder() => 0;

        public abstract bool CanChangeState(TStateDataType stateData);
        public abstract void ChangeState(TStateDataType stateData);
    }
}