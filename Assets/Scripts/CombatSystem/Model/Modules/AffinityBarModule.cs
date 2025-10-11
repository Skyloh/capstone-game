using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

// Left to Right
public class AffinityBarModule : AModule
{
    public delegate void ChangeAffinityBar(IList<AffinityType> current, IList<AffinityType> previous);

    private readonly IList<AffinityType> m_barSequence;

    public event ChangeAffinityBar OnAffinityBarChanged;

    private readonly int m_slotCount;

    public AffinityBarModule(int slot_count)
    {
        m_barSequence = new List<AffinityType>();
        m_slotCount = slot_count;

        FillBar();
    }

    public void FillBar()
    {
        m_barSequence.Clear();

        for (int i = 0; i < m_slotCount; i++)
        {
            var affinity = Random.Range(0, 3) switch
            {
                0 => AffinityType.Red,
                1 => AffinityType.Blue,
                2 => AffinityType.Yellow,
                3 => AffinityType.Green,
                // unreachable
                _ => AffinityType.None,
            };

            m_barSequence.Add(affinity);
        }
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
        if (count == 0) return; 

        int start = GetFirstNonNoneIndex();
        for (int i = 0; i < count; ++i)
        {
            if (i + start >= BarLength()) break;

            SetAtIndex(i + start, AffinityType.None);
        }

        if (IsBroken() && GetOwner().TryGetModule<StatusModule>(out var status_module))
        {
            status_module.AddStatus(StatusModule.Status.Stun, 1);
        }
    }

    public bool IsBroken()
    {
        return GetFirstNonNoneIndex() == -1;
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
