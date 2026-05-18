using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace TransitionStateMachine.EnumStates
{
    public class StateMachine<TStateType, TStateDataType> where TStateType : Enum
    {
        public TStateType CurrentState { get; private set; }

        private Dictionary<TStateType, List<BaseTransition<TStateType, TStateDataType>>> _transitionGroupsByFromState;

        private readonly DiContainer _container;
        private readonly HashSet<Type> _transitionTypes;

        private List<BaseTransition<TStateType, TStateDataType>> _startTransitions;

        private bool _stateSelected;

        public StateMachine(DiContainer container, HashSet<Type> transitionTypes)
        {
            _container = container;

            _transitionTypes = transitionTypes;

            _stateSelected = false;
        }

        public void UpdateState(TStateDataType stateData)
        {
            AssertThatTransitionGroupsInitialized();

            if (_stateSelected == false)
            {
                BaseTransition<TStateType, TStateDataType> startTransition = 
                    SelectTransitionWithLargestOrderValue(_startTransitions.Where(tr => tr.CanChangeState(stateData)).ToArray());

                ChangeState(startTransition, stateData);
                _stateSelected = true;
                return;
            }

            if (_transitionGroupsByFromState.ContainsKey(CurrentState) == false)
            {
                UnityEngine.Debug.LogWarning($"State {CurrentState} have not any transitions");
                return;
            }

            BaseTransition<TStateType, TStateDataType> currentTransition = 
                SelectTransitionWithLargestOrderValue(_transitionGroupsByFromState[CurrentState]
                .Where(tr => tr.CanChangeState(stateData)).ToArray());

            if (currentTransition != null)
            {
                ChangeState(currentTransition, stateData);
            }
        }

        private BaseTransition<TStateType, TStateDataType> SelectTransitionWithLargestOrderValue(IEnumerable<BaseTransition<TStateType, TStateDataType>> transitions)
        {
            if (transitions.Any() == false)
                return null;
            
            BaseTransition<TStateType, TStateDataType> needTransition = transitions
                .OrderBy(tr => tr.GetTransitionOrder())
                .Last();

            return needTransition;
        }

        private void ChangeState(BaseTransition<TStateType, TStateDataType> transition, TStateDataType stateData)
        {
            transition.ChangeState(stateData);
            CurrentState = transition.ToState;
        }

        private void AssertThatTransitionGroupsInitialized()
        {
            if (_transitionGroupsByFromState != null)
                return;

            if (_transitionTypes == null)
                throw new ArgumentNullException();

            InstantiateTransitions(_transitionTypes);
        }

        private void InstantiateTransitions(IEnumerable<Type> transitionTypes)
        {
            IEnumerable<BaseTransition<TStateType, TStateDataType>> transitions = transitionTypes
                .Select(type => InstantiateTransition(type));

            RegisterTransitions(transitions);
        }

        private BaseTransition<TStateType, TStateDataType> InstantiateTransition(Type transitionType)
        {
            if (typeof(BaseTransition<TStateType, TStateDataType>).IsAssignableFrom(transitionType) == false)
                throw new ArgumentException($"Uncorrect transition type {transitionType}");

            return _container.Instantiate(transitionType) as BaseTransition<TStateType, TStateDataType>;
        }

        private void RegisterTransitions(IEnumerable<BaseTransition<TStateType, TStateDataType>> transitions)
        {
            _transitionGroupsByFromState = new();
            Dictionary<TStateType, HashSet<TStateType>> tempGroupsByFromState = new();

            _startTransitions = new();
            HashSet<TStateType> tempStartTransitions = new();

            foreach (BaseTransition<TStateType, TStateDataType> transition in transitions)
            {
                if (transition.FromStates == null)
                {
                    if (tempStartTransitions.Contains(transition.ToState))
                        throw new ArgumentException();

                    _startTransitions.Add(transition);
                    tempStartTransitions.Add(transition.ToState);

                    continue;
                }

                RegisterTransition(transition, ref tempGroupsByFromState);
            }
        }

        private void RegisterTransition(BaseTransition<TStateType, TStateDataType> transition, ref Dictionary<TStateType, HashSet<TStateType>> tempGroupsByFromState)
        {
            AssertThatDictionaryContainsAllFromStates(transition);

            AssertThatTempGroupContainsAllFromStates(transition, ref tempGroupsByFromState);

            AssertThatTransitionWithCurrentSignatureNotExist(transition, ref tempGroupsByFromState);

            foreach (TStateType fromState in transition.FromStates)
            {
                _transitionGroupsByFromState[fromState].Add(transition);
                tempGroupsByFromState[fromState].Add(transition.ToState);
            }
        }

        private void AssertThatDictionaryContainsAllFromStates(BaseTransition<TStateType, TStateDataType> transition)
        {
            foreach (TStateType state in transition.FromStates)
            {
                if (_transitionGroupsByFromState.ContainsKey(state) == false)
                    _transitionGroupsByFromState.Add(state, new());
            }
        }

        private void AssertThatTempGroupContainsAllFromStates(BaseTransition<TStateType, TStateDataType> transition, ref Dictionary<TStateType, HashSet<TStateType>> tempGroupsByFromState)
        {
            foreach (TStateType state in transition.FromStates)
            {
                if (tempGroupsByFromState.ContainsKey(state) == false)
                    tempGroupsByFromState.Add(state, new());
            }
        }

        private void AssertThatTransitionWithCurrentSignatureNotExist(BaseTransition<TStateType, TStateDataType> transition, ref Dictionary<TStateType, HashSet<TStateType>> tempGroupsByFromState)
        {
            foreach (TStateType fromState in transition.FromStates)
            {
                if (tempGroupsByFromState[fromState].Contains(transition.ToState))
                    throw new Exception();
            }
        }
    }
}
