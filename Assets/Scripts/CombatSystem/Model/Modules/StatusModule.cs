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
        Chill
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

        OnEffectChanged.Invoke((status, m_statusDurationMap[status]), (Status.None, -1));
    }

    public void DecrementStatusDuration(Status status, int by_amount = 1)
    {
        m_statusDurationMap[status] -= by_amount;

        if (m_statusDurationMap[status] == 0)
        {
            m_statusDurationMap.Remove(status);

            OnEffectChanged.Invoke((status, m_statusDurationMap[status] + by_amount), (Status.None, -1));
        }
        else
        {
            OnEffectChanged.Invoke((status, m_statusDurationMap[status] + by_amount), (status, m_statusDurationMap[status]));
        }
    }

    public ICollection<Status> GetStatuses()
    {
        return m_statusDurationMap.Keys;
    }

    public bool HasStatus(Status status) => m_statusDurationMap.ContainsKey(status);
}
