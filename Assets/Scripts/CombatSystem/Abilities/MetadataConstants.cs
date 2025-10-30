public static class MetadataConstants
{
    // the strings themselves are arranged to have their most unique character first, as to
    // slightly increase the speed of the equalities we're doing.
    // ...you're never meant to see the string data themselves anyways, so it's fine for them to be weird :>
    public const string WEAPON_ELEMENT = "element-weapon";

    public const string WEAPON_OR_WEAKNESS = "or-weapon-weakness";
    public const string WEAPON = "weapon";
    public const string WEAKNESS = "weakness";

    public const string AFF_INDEX_TARGET_INDEX = "index-element-index-target";
    public const string OPTIONAL_AITI = "optional-" + AFF_INDEX_TARGET_INDEX;

    public const string PAIR_AFF_INDEX_TARGET_INDEX = "pair-index-element-index-target";

    /* If i need these, use them. Try to get a more generalized key implementation first, though. These are bespoke.
    public const string AFFINITY_INDEX_2 = "2-affinity-index"; // for Swap (pick 2 indices on up to 2 enemies)
    public const string AFFINITY_INDEX_3 = "3-affinity-index"; // for Delay (pick 3 indices on 1 enemy)
    public const string AFFINITY_INDEX_3_CONTIG = "contig-3-affinity-index"; // for HueShift (pick 3 contiguous elements on 1 enemy)
    public const string AFFINITY_PAIR_INDEX_2 = "pair-2-affinity-index"; // for Seg Swap (pick 2 indicies on 1 enemy)
    */
}

// public enum MetadataConstants
// {
//     WEAPON_ELEMENT,
//     WEAPON,
//     WEAKNESS,
//     AFF_INDEX_TARGET_INDEX,
//     OPTIONAL_AITI,
//     PAIR_AFF_INDEX_TARGET_INDEX,
// }