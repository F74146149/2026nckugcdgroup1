using UnityEngine;

[System.Serializable]
public class AttackData
{
    public float damage;
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

    [Header("Size")]
    [Tooltip("武器在畫面上的長度（Unity 單位）。圖片會縮放成這個長度，攻擊範圍就是圖片的形狀")]
    [Min(0.1f)] [SerializeField] private float weaponLength = 2f;

    public string WeaponName => weaponName;
    public float WeaponLength => weaponLength;
    public Sprite WeaponSprite => weaponSprite;
    public float ComboInputWindow => comboInputWindow;
    public AttackData[] ComboAttacks => comboAttacks;

    public float ComboResetDuration => comboResetDuration;
}
