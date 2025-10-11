using System.Collections.Generic;

public class StatusModule : AModule
{
    public enum Status
    { 
        None,
        Stun,
        Burn,
        Shock,
        Bruise,
        Chill,
        MorphRed, // change weapon element to X
        MorphBlue,
        MorphYellow,
        MorphGreen,
        MorphNone,
        VeilRed, // change weakness element to X
        VeilBlue,
        VeilYellow,
        VeilGreen,
        VeilNone
    }

    public delegate void ChangeEffect((Status status, int duration) from, (Status status, int duration) to);

    private readonly IDictionary<Status, int> m_statusDurationMap;

    public event ChangeEffect OnEffectChanged; // for View stuff. Dont forget to desubscribe!

    public StatusModule()
    {
        m_statusDurationMap = new Dictionary<Status, int>();
    }

    public void AddStatus(Status status, int duration)
    {


        m_statusDurationMap[status] = duration;

        OnEffectChanged?.Invoke((status, m_statusDurationMap[status]), (Status.None, -1));
    }

    public void DecrementStatusDuration(Status status, int by_amount = 1)
    {
        m_statusDurationMap[status] -= by_amount;

        if (m_statusDurationMap[status] == 0)
        {
            m_statusDurationMap.Remove(status);

            OnEffectChanged?.Invoke((status, m_statusDurationMap[status] + by_amount), (Status.None, -1));
        }
        else
        {
            OnEffectChanged?.Invoke((status, m_statusDurationMap[status] + by_amount), (status, m_statusDurationMap[status]));
        }
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

    private bool IsMorphStatus(Status status) => (int)status >= 6 && (int)status <= 10;
    private bool IsVeilStatus(Status status) => (int)status >= 11 && (int)status <= 11;
}
