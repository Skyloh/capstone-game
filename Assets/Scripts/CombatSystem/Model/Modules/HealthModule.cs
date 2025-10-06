using UnityEngine;

public class HealthModule : IModule
{
    public delegate void ChangeStat(int max, int current, int previous);

    private readonly int m_maxHealth;
    private int m_currentHealth;

    public event ChangeStat OnHealthChanged; // for View stuff. Dont forget to desubscribe!

    public HealthModule(int m_hp, int c_hp)
    {
        m_maxHealth = m_hp;
        m_currentHealth = c_hp;
    }

    public void SetHealth(int health)
    {
        int health_cache = m_currentHealth;

        m_currentHealth = Mathf.Max(Mathf.Min(health, m_maxHealth), 0); // bounds-clamping health

        OnHealthChanged?.Invoke(m_maxHealth, health, health_cache);
    }

    public void ChangeHealth(int decrease_amount) => SetHealth(m_currentHealth - decrease_amount);

    public bool IsAlive() => m_currentHealth > 0;
}
