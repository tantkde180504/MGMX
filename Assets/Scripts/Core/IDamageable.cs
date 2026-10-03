using UnityEngine;

namespace MMX.Core
{
    /// <summary>
    /// Giao diện cho bất kỳ thực thể nào có thể nhận sát thương (Player, Quái, Boss).
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int damage, Vector2 hitDirection, float knockbackForce = 0f);
        bool IsDead { get; }
        int CurrentHealth { get; }
        int MaxHealth { get; }
    }
}
