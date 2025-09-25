using System.Collections.Generic;

public class AffinityBarModule : IModule
{
    public delegate void ChangeAffinityBar(IList<AffinityType> current, IList<AffinityType> previous);

    private readonly IList<AffinityType> m_barSequence;

    public event ChangeAffinityBar OnAffinityBarChanged;

    public AffinityType GetAtIndex(int i) => m_barSequence[i];

    public AffinityBarModule(IList<AffinityType> bar_sequence)
    {
        m_barSequence = bar_sequence;
    }

    public void SetAtIndex(int i, AffinityType type)
    {
        var clone = new List<AffinityType>(m_barSequence);

        m_barSequence[i] = type;

        OnAffinityBarChanged.Invoke(m_barSequence, clone);
    }

    // helpers for other functionality eventually
}
