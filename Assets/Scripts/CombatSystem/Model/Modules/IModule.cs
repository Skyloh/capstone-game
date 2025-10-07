/// <summary>
/// Defines an interface for data blocks that go on CombatUnits.
/// </summary>
public interface IModule
{
    void SetOwner(CombatUnit unit);
    CombatUnit GetOwner();
}
