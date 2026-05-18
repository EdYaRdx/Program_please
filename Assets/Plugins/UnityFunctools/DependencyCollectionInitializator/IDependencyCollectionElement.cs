using System;
using System.Collections.Generic;

namespace UnityFunctools
{
    public interface IDependencyCollectionElement
    {
        HashSet<Type> GetAllProvidedContracts();
    }
}
