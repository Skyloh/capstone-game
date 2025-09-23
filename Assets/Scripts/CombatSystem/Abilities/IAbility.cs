using System.Collections;
public interface IAbility
{
    IEnumerator IE_ProcessAbility(ActionData data, ICombatModel model, ICombatView view);
}
