using UnityEngine;

public class BattlebackSpriteManager : MonoBehaviour
{
    [Header("Background Sprites")]
    [SerializeField] private Sprite background1;
    [SerializeField] private Sprite background2;
    [SerializeField] private Sprite background3;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    /*
    // Switch by index (currently unused)
    public void SetBattlebackByIndex(int index)
    {
        Sprite[] arr = { background1, background2, background3 };
        if (index >= 0 && index < arr.Length)
            sr.sprite = arr[index];
    }

    // Switch by name 
    public void SetBattlebackByName(string name)
    {
        switch (name.ToLower())
        {
            case "bg1":
            case "background1":
                sr.sprite = background1;
                break;
            case "bg2":
            case "background2":
                sr.sprite = background2;
                break;
            case "bg3":
            case "background3":
                sr.sprite = background3;
                break;
        }
    }
    */

    //  Random background 
    public void SetRandomBattleback()
    {
        int r = Random.Range(0, 3);
        Sprite[] arr = { background1, background2, background3 };
        sr.sprite = arr[r];
    }
}
