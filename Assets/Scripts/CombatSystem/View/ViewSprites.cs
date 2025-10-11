using UnityEngine;
[CreateAssetMenu(fileName = "BattleSprites",menuName="ViewSprites")]
public class ViewSprites : ScriptableObject
{
    [SerializeField]
    public Sprite firePlayerWeakness;
    [SerializeField]
    public Sprite waterPlayerWeakness;
    [SerializeField]
    public Sprite lightningPlayerWeakness;
    [SerializeField]
    public Sprite physicalPlayerWeakness;
    
    [SerializeField]
    public Sprite fireEnemyWeakness;
    [SerializeField]
    public Sprite waterEnemyWeakness;
    [SerializeField]
    public Sprite lightningEnemyWeakness;
    [SerializeField] 
    public Sprite physicalEnemyWeakness;
}
