using System;
using System.Collections.Generic;
using UnityEngine;
using UnityFunctools;

namespace MVCSample.Tools.Test
{
    public class TestInitSystem : MonoBehaviour
    {
        private Human[] _humans;

        private void Awake()
        {
            _humans = new Human[]
            {
                new Worker(),
                new Warrior(),
                new King()
            };
        }

        private void Start()
        {
            //DependencyCollectionInitializator<Human> initializator = new(_humans, h => h.Initialize());

            //initializator.Initialize();
        }
    }

    public abstract class Human : IDependencyCollectionElement
    {
        public virtual HashSet<Type> GetAllProvidedContracts() => new HashSet<Type> { GetType() };

        public abstract HashSet<Type> GetNecessaryDependencesInCurrentContext();

        public virtual void Initialize()
        {
            Debug.Log($"{GetType()} was init");
        }
    }

    public interface IWarlike
    {

    }

    public interface IPeaceful
    {

    }

    public class Worker : Human, IPeaceful
    {
        public override HashSet<Type> GetNecessaryDependencesInCurrentContext()
        {
            return new HashSet<Type> { typeof(King) };
        }

        public override HashSet<Type> GetAllProvidedContracts()
        {
            return new HashSet<Type> { typeof(IPeaceful) };
        }
    }

    public class Warrior : Human, IWarlike
    {
        public override HashSet<Type> GetNecessaryDependencesInCurrentContext()
        {
            return new(new Type[] { typeof(IPeaceful) });
        }

        public override HashSet<Type> GetAllProvidedContracts()
        {
            return new HashSet<Type> { typeof(IWarlike) };
        }
    }

    public class King : Human, IPeaceful
    {
        public override HashSet<Type> GetNecessaryDependencesInCurrentContext()
        {
            return new HashSet<Type> { typeof(IPeaceful), typeof(IWarlike) };
        }
    }
}
