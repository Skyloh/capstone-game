using System.Collections.Generic;

public class StatusModule : AModule
{
    public delegate void ChangeEffect((Status status, int duration) from, (Status status, int duration) to);

    private readonly IDictionary<Status, int> m_statusDurationMap;

    public event ChangeEffect OnEffectChanged; // for View stuff. Dont forget to desubscribe!

    public StatusModule()
    {
        m_statusDurationMap = new Dictionary<Status, int>();
    }

    public void AddStatus(Status status, int duration)
    {
        if (status == Status.None) return;

        // only 1 morph can be active at a time; only 1 veil can be active at a time
        // the most recently-applied takes priority.
        EnsureMorphVeilInvariant(status);

        m_statusDurationMap[status] = duration;

        OnEffectChanged?.Invoke((Status.None, -1), (status, m_statusDurationMap[status]));
    }

    public void DecrementStatusDuration(Status status, int by_amount = 1)
    {
        m_statusDurationMap[status] -= by_amount;

        if (m_statusDurationMap[status] == 0)
        {
            RemoveStatus(status, by_amount);
        }
        else
        {
            OnEffectChanged?.Invoke((status, m_statusDurationMap[status] + by_amount), (status, m_statusDurationMap[status]));
        }
    }

    private void RemoveStatus(Status status, int decrement_amount = 0)
    {
        int duration_remaining = m_statusDurationMap[status];

        m_statusDurationMap.Remove(status);

        OnEffectChanged?.Invoke((status, duration_remaining + decrement_amount), (Status.None, -1));
    }

    public ICollection<Status> GetStatuses()
    {
        return m_statusDurationMap.Keys;
    }

    public bool HasStatus(Status status)
    {
        return m_statusDurationMap.ContainsKey(status);
    }

    public Status GetContainedMorphStatus()
    {
        if (m_statusDurationMap.ContainsKey(Status.MorphBlue)) return Status.MorphBlue;
        else if (m_statusDurationMap.ContainsKey(Status.MorphGreen)) return Status.MorphGreen;
        else if (m_statusDurationMap.ContainsKey(Status.MorphRed)) return Status.MorphRed;
        else if (m_statusDurationMap.ContainsKey(Status.MorphYellow)) return Status.MorphYellow;
        else if (m_statusDurationMap.ContainsKey(Status.MorphNone)) return Status.MorphNone;

        return Status.None;
    }

    public Status GetContainedVeilStatus()
    {
        if (m_statusDurationMap.ContainsKey(Status.VeilBlue)) return Status.VeilBlue;
        else if (m_statusDurationMap.ContainsKey(Status.VeilGreen)) return Status.VeilGreen;
        else if (m_statusDurationMap.ContainsKey(Status.VeilRed)) return Status.VeilRed;
        else if (m_statusDurationMap.ContainsKey(Status.VeilYellow)) return Status.VeilYellow;
        else if (m_statusDurationMap.ContainsKey(Status.VeilNone)) return Status.VeilNone;

        return Status.None;
    }

    public static bool IsEmptyStatus((Status s, int dur) item) => item.s == Status.None || item.dur == -1;

    private void EnsureMorphVeilInvariant(Status incoming)
    {
        // no dupe Morphs; only most recent applied
        Status contained_m = GetContainedMorphStatus();
        if (StatusUtils.IsMorphStatus(incoming) && contained_m != Status.None)
        {
            RemoveStatus(contained_m);
        }

        // no dupe Veils; only most recent applied
        Status contained_v = GetContainedVeilStatus();
        if (StatusUtils.IsVeilStatus(incoming) && contained_v != Status.None)
        {
            RemoveStatus(contained_v);
        }
    }
}
