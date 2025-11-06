using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// A factory class that creates Ability instances from strings.
/// </summary>
public class AbilityFactory
{
    /// <summary>
    /// Given an array of namespace qualified typenames (i.e. just the names of the Abilities),
    /// returns a list of the constructed ability instances. The constructor must have no arguments,
    /// which is the default for all abilities.
    /// </summary>
    /// <param name="namespace_qualified_typenames"></param>
    /// <returns></returns>
    public static IList<IAbility> MakeAbilities(string[] namespace_qualified_typenames)
    {
        var assembly = typeof(AttackAbility).Assembly;
        var list = new List<IAbility>();

        foreach (var type_string in namespace_qualified_typenames)
        {
            list.Add((IAbility)GetConstructor(assembly, type_string).Invoke(null));
        }

        return list;
    }

    public static bool AssertValid(string namespace_qualified_typename)
    {
        try
        {
            GetConstructor(typeof(AttackAbility).Assembly, namespace_qualified_typename);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Ability name {namespace_qualified_typename} is not valid. Reason: {e.Message}");
            return false;
        }

        return true;
    }

    private static ConstructorInfo GetConstructor(Assembly in_assembly, string name)
    {
        var type = in_assembly.GetType(name);
        return type.GetConstructor(new Type[0]);
    }
}
