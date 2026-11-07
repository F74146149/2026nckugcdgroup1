using UnityEngine;

[System.Serializable]
public class AttackData
{
    public int damage;
    public float duration;
    public float startAngle;
    public float endAngle;
}

[CreateAssetMenu(fileName ="New Weapon", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [SerializeField]private AttackData[] comboAttacks;

    public AttackData[] ComboAttacks => comboAttacks;
}