using System;
using Godot;

[GlobalClass]
public partial class TypeReference : Resource
{
    [Export]
    private string assemblyQualifiedName;

    private Type _cachedType;

    public Type Type
    {
        get
        {
            if (_cachedType == null && !string.IsNullOrEmpty(assemblyQualifiedName))
                _cachedType = Type.GetType(assemblyQualifiedName);
            return _cachedType;
        }
        set
        {
            _cachedType = value;
            assemblyQualifiedName = value?.AssemblyQualifiedName;
        }
    }
}