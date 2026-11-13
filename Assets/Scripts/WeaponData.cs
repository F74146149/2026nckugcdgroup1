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
    [Header("WeaponInformation")]
    [SerializeField]private string weaponName;
    [SerializeField]private Sprite weaponSprite;

    [Header("combo Setting")]
    [SerializeField]private AttackData[] comboAttacks;
    [SerializeField]private float comboResetDuration = 0.5f;
    [SerializeField] private float comboInputWindow = 0.15f;

    public string WeaponName => weaponName;
    public Sprite WeaponSprite => weaponSprite;
    public float ComboInputWindow => comboInputWindow;
    public AttackData[] ComboAttacks => comboAttacks;

    public float ComboResetDuration => comboResetDuration;
}
