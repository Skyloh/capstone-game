using System.Collections.Generic;

public class StatusModule : AModule
{
    // TEMPORARY
    public enum TEMP_Status
    { None }

    public delegate void ChangeEffect((TEMP_Status status, int duration) from, (TEMP_Status status, int duration) to);

    private readonly IDictionary<TEMP_Status, int> m_statusDurationMap;

    public event ChangeEffect OnEffectChanged; // for View stuff. Dont forget to desubscribe!

    public StatusModule()
    {
        m_statusDurationMap = new Dictionary<TEMP_Status, int>();
    }

    public void DecrementStatusDuration(TEMP_Status status, int by_amount = 1)
    {
        m_statusDurationMap[status] -= by_amount;

        OnEffectChanged.Invoke((status, m_statusDurationMap[status] + by_amount), (status, m_statusDurationMap[status]));
    }
}
