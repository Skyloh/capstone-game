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

    private IList<AffinityType> m_bookmark;

    public AffinityBarModule(int slot_count)
    {
        m_barSequence = new List<AffinityType>();
        m_slotCount = slot_count;

        FillBar();
    }

    // DEBUG
    public AffinityBarModule(AffinityType type, int slot_count)
    {
        m_barSequence = new List<AffinityType>();

        for (int i = 0; i < slot_count; i++) m_barSequence.Add(type);

        m_slotCount = slot_count;
    }

    public void FillBar()
    {
        Bookmark();

        m_barSequence.Clear();

        for (int i = 0; i < m_slotCount; i++)
        {
            var affinity = Random.Range(0, 4) switch
            {
                0 => AffinityType.Fire,
                1 => AffinityType.Water,
                2 => AffinityType.Lightning,
                3 => AffinityType.Physical,
                // unreachable
                _ => AffinityType.None,
            };

            m_barSequence.Add(affinity);
        }

        ConsumeBookmark();
    }

    public AffinityType this[int index]
    {
        get => GetAtIndex(index);
        set => SetAtIndex(index, value);
    }

    public int BarLength() => m_barSequence.Count;

    public AffinityType GetAtIndex(int i) => m_barSequence[i];

    public void SetAtIndex(int i, AffinityType type)
    {
        Bookmark();

        m_barSequence[i] = type;

        ConsumeBookmark();
    }

    public void Bookmark() => m_bookmark = new List<AffinityType>(m_barSequence);
    public IList<AffinityType> GetAffinities() => m_barSequence;
    public void ConsumeBookmark()
    {
        OnAffinityBarChanged?.Invoke(m_barSequence, m_bookmark);
        m_bookmark = null;
    }


    // VERY IMPORTANT to call Bookmark and ConsumeBookmark before and after using the Silent methods
    public void SilentRemoveAt(int index) => m_barSequence.RemoveAt(index);

    public void SilentPushBack(AffinityType aff) => m_barSequence.Add(aff);

    public int GetFirstNonNoneIndex()
    {
        for (int i = 0; i < m_barSequence.Count; i++)
        {
            if (GetAtIndex(i) != AffinityType.None) return i;
        }

        return -1;
    }

    public IList<AffinityType> CloneCurrentBar() => new List<AffinityType>(m_barSequence);

    public void BreakLeading(int count)
    {
        if (count == 0) return;

        int start = GetFirstNonNoneIndex();

        // if the full bar is already broken, dont continue.
        if (start == -1) return;

        for (int i = 0; i < count; ++i)
        {
            if (i + start >= BarLength()) break;

            SetAtIndex(i + start, AffinityType.None);
        }

        if (IsBroken() && GetOwner().TryGetModule<StatusModule>(out var status_module))
        {
            status_module.AddStatus(Status.Stun, 2);
        }
    }

    public bool IsBroken()
    {
        return GetFirstNonNoneIndex() == -1;
    }

    public int CalculateLeadingBreaks(AffinityType break_element)
    {
        return CalculateLeadingBreaks(new HashSet<AffinityType>() { break_element });
    }

    public int CalculateLeadingBreaks(ISet<AffinityType> break_elements)
    {
        int breaks = 0;
        for (int i = GetFirstNonNoneIndex();
             i >= 0 && i < BarLength() && break_elements.Contains(GetAtIndex(i));
             ++i)
        {
            ++breaks;
        }

        return breaks;
    }

    public IReadOnlyList<AffinityType> GetSubrange(int start, int end)
    {
        var range = new List<AffinityType>();

        for (int i = start; i < end && i < BarLength(); ++i)
        {
            range.Add(GetAtIndex(i));
        }

        return range;
    }

    // helpers for other functionality eventually
}