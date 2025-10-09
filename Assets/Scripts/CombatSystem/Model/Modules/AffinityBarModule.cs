using System.Collections.Generic;
using System.Runtime.InteropServices;

// Left to Right
public class AffinityBarModule : AModule
{
    public delegate void ChangeAffinityBar(IList<AffinityType> current, IList<AffinityType> previous);

    private readonly IList<AffinityType> m_barSequence;

    public event ChangeAffinityBar OnAffinityBarChanged;

    public AffinityBarModule(IList<AffinityType> bar_sequence)
    {
        m_barSequence = bar_sequence;
    }

    public int BarLength() => m_barSequence.Count;

    public AffinityType GetAtIndex(int i) => m_barSequence[i];

    public void SetAtIndex(int i, AffinityType type)
    {
        var clone = new List<AffinityType>(m_barSequence);

        m_barSequence[i] = type;

        OnAffinityBarChanged.Invoke(m_barSequence, clone);
    }

    public int GetFirstNonNoneIndex()
    {
        for (int i = 0; i < m_barSequence.Count; i++)
        {
            if (GetAtIndex(i) != AffinityType.None) return i;
        }

        return -1;
    }

    public void BreakLeading(int count)
    {
        int start = GetFirstNonNoneIndex();
        for (int i = 0; i < count; ++i)
        {
            if (i + start >= BarLength()) break;

            SetAtIndex(i + start, AffinityType.None);
        }
    }

    public int CalculateLeadingBreaks(AffinityType break_element)
    {
        int breaks = 0;
        for (int i = GetFirstNonNoneIndex();
            i >= 0 && i < BarLength() && GetAtIndex(i) == break_element;
            ++i)
        {
            ++breaks;
        }

        return breaks;
    }

    // helpers for other functionality eventually
}
