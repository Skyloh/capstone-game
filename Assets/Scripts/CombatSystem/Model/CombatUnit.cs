using UnityEngine;

public class CombatUnit
{
    public delegate void ChangeStat(int current, int previous);

    private int m_maxHealth;
    private int m_currentHealth;

    public event ChangeStat OnHealthChanged; // for View stuff.

    public CombatUnit(int m_hp, int c_hp)
    {
        m_maxHealth = m_hp;
        m_currentHealth = c_hp;
    }

    public CombatUnit(int m_hp) : this(m_hp, m_hp) { }

    public void SetHealth(int health)
    {
        int health_cache = m_currentHealth;

        m_currentHealth = Mathf.Max(Mathf.Min(health, m_maxHealth), 0); // bounds-clamping health

        OnHealthChanged?.Invoke(health, health_cache);
    }

    public void ChangeHealth(int decrease_amount) => SetHealth(m_currentHealth - decrease_amount);

    public bool IsAlive() => m_currentHealth > 0;

}
