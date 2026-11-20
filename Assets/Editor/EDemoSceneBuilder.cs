using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 組員 E 的編輯器小工具（只在 Unity 編輯器裡用，不會進到遊戲裡）。
///
/// 上方選單 Tools → 組員E：
///   建立範例被動能力   → 在 Assets/Data/Passives 產生 8 個埃及風被動能力
///   建立商店測試場景   → 產生 Assets/Scenes/E_ShopTest.unity，HUD、商店、勝敗畫面都接好
///
/// 想重做測試場景時，再按一次就會覆蓋（被動能力資料不會被覆蓋）。
/// </summary>
public static class EDemoSceneBuilder
{
    private const string DataFolder = "Assets/Data";
    private const string PassiveFolder = "Assets/Data/Passives";
    private const string ScenePath = "Assets/Scenes/E_ShopTest.unity";

    private static readonly Color Gold = new Color(1f, 0.82f, 0.3f);
    private static readonly Color Sand = new Color(0.78f, 0.62f, 0.3f);
    private static readonly Color DarkText = new Color(0.12f, 0.09f, 0.05f);
    private static readonly Color CardInner = new Color(0.13f, 0.11f, 0.09f, 1f);

    // ================= 選單 =================

    [MenuItem("Tools/組員E/建立範例被動能力")]
    public static void CreateSamplePassivesMenu()
    {
        List<PassiveData> list = CreateSamplePassives();
        Debug.Log("組員E：範例被動能力已就緒，共 " + list.Count + " 個，位置：" + PassiveFolder);
        if (list.Count > 0) EditorGUIUtility.PingObject(list[0]);
    }

    [MenuItem("Tools/組員E/建立商店測試場景")]
    public static void BuildShopTestSceneMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        BuildShopTestScene();
    }

    /// <summary>給命令列（batchmode）用：兩件事一起做</summary>
    public static void BuildAllBatch()
    {
        CreateSamplePassives();
        BuildShopTestScene();
    }

    // ================= 被動能力 =================

    public static List<PassiveData> CreateSamplePassives()
    {
        EnsureFolder(DataFolder);
        EnsureFolder(PassiveFolder);

        var list = new List<PassiveData>
        {
            MakePassive("Passive_AnkhOfLife", "Ankh of Life", 20, Rarity.Common, 0,
                Mod(StatType.MaxHealth, ModifierKind.Flat, 1f)),

            MakePassive("Passive_KhopeshEdge", "Khopesh Edge", 25, Rarity.Common, 0,
                Mod(StatType.Attack, ModifierKind.Percent, 0.2f)),

            MakePassive("Passive_SandWalker", "Sand Walker", 20, Rarity.Common, 0,
                Mod(StatType.MoveSpeed, ModifierKind.Percent, 0.15f)),

            MakePassive("Passive_GoldenMask", "Golden Mask", 30, Rarity.Rare, 0,
                Mod(StatType.Loot, ModifierKind.Flat, 0.25f)),

            MakePassive("Passive_ScarabCharm", "Scarab Charm", 30, Rarity.Rare, 0,
                Mod(StatType.Luck, ModifierKind.Flat, 1f)),

            MakePassive("Passive_EyeOfRa", "Eye of Ra", 35, Rarity.Rare, 0,
                Mod(StatType.CritChance, ModifierKind.Flat, 0.05f)),

            MakePassive("Passive_AnubisBlessing", "Anubis' Blessing", 60, Rarity.Epic, 1,
                Mod(StatType.MaxHealth, ModifierKind.Flat, 2f),
                Mod(StatType.Attack, ModifierKind.Percent, 0.1f)),

            // 風險與報酬：攻擊大增，但少一顆心
            MakePassive("Passive_SekhmetFury", "Sekhmet's Fury", 55, Rarity.Epic, 1,
                Mod(StatType.Attack, ModifierKind.Percent, 0.5f),
                Mod(StatType.MaxHealth, ModifierKind.Flat, -1f)),
        };

        AssetDatabase.SaveAssets();
        return list;
    }

    private static StatModifier Mod(StatType stat, ModifierKind kind, float value)
    {
        return new StatModifier(stat, kind, value);
    }

    private static PassiveData MakePassive(string fileName, string displayName, int price, Rarity rarity,
        int maxStacks, params StatModifier[] modifiers)
    {
        string path = PassiveFolder + "/" + fileName + ".asset";

        // 已經存在就不覆蓋（可能有人調過數值）
        PassiveData existing = AssetDatabase.LoadAssetAtPath<PassiveData>(path);
        if (existing != null) return existing;

        PassiveData p = ScriptableObject.CreateInstance<PassiveData>();
        p.displayName = displayName;
        p.description = "";
        p.price = price;
        p.rarity = rarity;
        p.maxStacks = maxStacks;
        p.modifiers = new List<StatModifier>(modifiers);

        AssetDatabase.CreateAsset(p, path);
        return p;
    }

    private static List<PassiveData> FindAllPassives()
    {
        var list = new List<PassiveData>();
        foreach (string guid in AssetDatabase.FindAssets("t:PassiveData"))
        {
            PassiveData p = AssetDatabase.LoadAssetAtPath<PassiveData>(AssetDatabase.GUIDToAssetPath(guid));
            if (p != null) list.Add(p);
        }
        return list;
    }

    // ================= 測試場景 =================

    public static void BuildShopTestScene()
    {
        // 先開新場景再載入資料：開新場景時 Unity 會把沒用到的資源從記憶體清掉
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        List<PassiveData> passives = FindAllPassives();
        if (passives.Count == 0) passives = CreateSamplePassives();

        // ---- 攝影機 ----
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(0f, 0f, -10f);
        Camera cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.16f, 0.13f, 0.1f);
        camGO.AddComponent<AudioListener>();

        // ---- EventSystem（新版輸入系統）----
        var esGO = new GameObject("EventSystem", typeof(EventSystem));
        InputSystemUIInputModule module = esGO.AddComponent<InputSystemUIInputModule>();
        module.AssignDefaultActions();

        // ---- 組員 E 的系統 ----
        var systems = new GameObject("RunSystems (組員E)");
        systems.AddComponent<CurrencyManager>();
        systems.AddComponent<PlayerStats>();
        systems.AddComponent<PassiveManager>();

        var shopGO = new GameObject("ShopManager");
        ShopManager shop = shopGO.AddComponent<ShopManager>();
        SetList(shop, "itemPool", passives);

        // ---- Canvas ----
        var canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        Transform canvas = canvasGO.transform;

        HealthUI healthUI = BuildHud(canvas);
        BuildShop(canvas, shop);
        BuildResultScreen(canvas);

        // ---- 測試腳本 ----
        var testers = new GameObject("Testers (測試用)");
        testers.AddComponent<CurrencyTester>();
        testers.AddComponent<ShopTester>();
        DemoPlayerHealth demo = testers.AddComponent<DemoPlayerHealth>();
        Set(demo, "healthUI", healthUI);

        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("組員E：測試場景已建立 → " + ScenePath);
    }

    // ---- HUD：愛心、金幣、能力值、操作提示 ----
    private static HealthUI BuildHud(Transform canvas)
    {
        RectTransform hud = NewUI("HUD", canvas);
        Stretch(hud);

        // 愛心
        RectTransform hearts = NewUI("HeartContainer", hud);
        Place(hearts, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -40), new Vector2(600, 60));
        HorizontalLayoutGroup hl = hearts.gameObject.AddComponent<HorizontalLayoutGroup>();
        hl.spacing = 10;
        hl.childAlignment = TextAnchor.MiddleLeft;
        hl.childControlWidth = false;
        hl.childControlHeight = false;
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = false;

        Image heart = NewImage("HeartTemplate", hearts, Color.white);
        heart.rectTransform.sizeDelta = new Vector2(48, 48);

        HealthUI healthUI = hud.gameObject.AddComponent<HealthUI>();
        Set(healthUI, "heartTemplate", heart);

        // 金幣
        TextMeshProUGUI coin = NewText("CoinText", hud, "Gold: 0", 44, TextAlignmentOptions.TopRight, Gold);
        Place(coin.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-40, -40), new Vector2(420, 60));
        coin.fontStyle = FontStyles.Bold;
        CoinUI coinUI = coin.gameObject.AddComponent<CoinUI>();
        Set(coinUI, "coinText", coin);

        // 能力值
        TextMeshProUGUI stats = NewText("StatsText", hud, "", 26, TextAlignmentOptions.TopLeft, new Color(0.92f, 0.88f, 0.8f));
        Place(stats.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(40, -130), new Vector2(460, 600));
        StatsPanelUI statsUI = stats.gameObject.AddComponent<StatsPanelUI>();
        Set(statsUI, "statsText", stats);

        // 操作提示
        TextMeshProUGUI hint = NewText("HintText", hud,
            "[G] +10 Gold    [O] Open Shop    [J] Take Damage    [H] Heal    [K] Game Over    [V] Victory",
            26, TextAlignmentOptions.Bottom, new Color(1f, 1f, 1f, 0.7f));
        Place(hint.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(1800, 60));

        return healthUI;
    }

    // ---- 商店 ----
    private static void BuildShop(Transform canvas, ShopManager shop)
    {
        RectTransform root = NewUI("ShopUI", canvas);
        Stretch(root);
        ShopUI shopUI = root.gameObject.AddComponent<ShopUI>();

        Image panel = NewImage("ShopPanel", root, new Color(0f, 0f, 0f, 0.82f));
        Stretch(panel.rectTransform);

        TextMeshProUGUI title = NewText("Title", panel.transform, "SHOP", 80, TextAlignmentOptions.Center, Gold);
        title.fontStyle = FontStyles.Bold;
        Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(800, 110));

        // 商品卡片排一排
        RectTransform slots = NewUI("Slots", panel.transform);
        Place(slots, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(1400, 460));
        HorizontalLayoutGroup hl = slots.gameObject.AddComponent<HorizontalLayoutGroup>();
        hl.spacing = 40;
        hl.childAlignment = TextAnchor.MiddleCenter;
        hl.childControlWidth = false;
        hl.childControlHeight = false;
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = false;

        ShopItemSlotUI template = BuildSlotTemplate(slots);

        // 提示文字
        TextMeshProUGUI message = NewText("Message", panel.transform, "", 32, TextAlignmentOptions.Center, Gold);
        Place(message.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 230), new Vector2(900, 50));

        // 按鈕
        TextMeshProUGUI rerollLabel;
        Button reroll = NewButton("RerollButton", panel.transform, "REROLL", out rerollLabel);
        Place((RectTransform)reroll.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-220, 110), new Vector2(380, 90));

        TextMeshProUGUI continueLabel;
        Button cont = NewButton("ContinueButton", panel.transform, "CONTINUE", out continueLabel);
        Place((RectTransform)cont.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(220, 110), new Vector2(380, 90));

        Set(shopUI, "shop", shop);
        Set(shopUI, "panel", panel.gameObject);
        Set(shopUI, "slotTemplate", template);
        Set(shopUI, "rerollButton", reroll);
        Set(shopUI, "rerollButtonText", rerollLabel);
        Set(shopUI, "continueButton", cont);
        Set(shopUI, "messageText", message);

        panel.gameObject.SetActive(false);
    }

    private static ShopItemSlotUI BuildSlotTemplate(Transform parent)
    {
        // 外框（稀有度顏色）
        Image frame = NewImage("SlotTemplate", parent, Sand);
        frame.rectTransform.sizeDelta = new Vector2(360, 450);
        ShopItemSlotUI slot = frame.gameObject.AddComponent<ShopItemSlotUI>();

        // 內底
        Image inner = NewImage("Inner", frame.transform, CardInner);
        Stretch(inner.rectTransform, 6f);
        inner.raycastTarget = false;
        Transform card = inner.transform;

        TextMeshProUGUI rarity = NewText("Rarity", card, "COMMON", 22, TextAlignmentOptions.Center, Sand);
        PlaceTop(rarity.rectTransform, -16, 34, 24);

        Image icon = NewImage("Icon", card, Sand);
        Place(icon.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(96, 96));
        icon.raycastTarget = false;

        TextMeshProUGUI itemName = NewText("Name", card, "Item Name", 30, TextAlignmentOptions.Center, Color.white);
        itemName.fontStyle = FontStyles.Bold;
        PlaceTop(itemName.rectTransform, -166, 44, 24);

        TextMeshProUGUI desc = NewText("Description", card, "+1 Max HP", 24, TextAlignmentOptions.Top, new Color(0.85f, 0.82f, 0.75f));
        PlaceTop(desc.rectTransform, -218, 100, 36);

        TextMeshProUGUI price = NewText("Price", card, "20 G", 32, TextAlignmentOptions.Center, Color.white);
        price.fontStyle = FontStyles.Bold;
        Place(price.rectTransform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 96), new Vector2(-24, 42));

        TextMeshProUGUI buyLabel;
        Button buy = NewButton("BuyButton", card, "BUY", out buyLabel);
        Place((RectTransform)buy.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 22), new Vector2(240, 64));

        // 賣掉後的遮罩
        Image sold = NewImage("SoldOverlay", frame.transform, new Color(0f, 0f, 0f, 0.6f));
        Stretch(sold.rectTransform);
        sold.raycastTarget = false;
        TextMeshProUGUI soldText = NewText("SoldText", sold.transform, "SOLD", 64, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.85f));
        soldText.fontStyle = FontStyles.Bold;
        Stretch(soldText.rectTransform);
        soldText.rectTransform.localRotation = Quaternion.Euler(0, 0, 15f);
        sold.gameObject.SetActive(false);

        Set(slot, "nameText", itemName);
        Set(slot, "descriptionText", desc);
        Set(slot, "priceText", price);
        Set(slot, "rarityText", rarity);
        Set(slot, "iconImage", icon);
        Set(slot, "frameImage", frame);
        Set(slot, "buyButton", buy);
        Set(slot, "buyButtonText", buyLabel);
        Set(slot, "soldOverlay", sold.gameObject);

        return slot;
    }

    // ---- 勝利 / 失敗 ----
    private static void BuildResultScreen(Transform canvas)
    {
        RectTransform root = NewUI("GameResultUI", canvas);
        Stretch(root);
        GameResultUI result = root.gameObject.AddComponent<GameResultUI>();

        Image panel = NewImage("ResultPanel", root, new Color(0f, 0f, 0f, 0.88f));
        Stretch(panel.rectTransform);

        TextMeshProUGUI title = NewText("Title", panel.transform, "YOU DIED", 120, TextAlignmentOptions.Center, Color.white);
        title.fontStyle = FontStyles.Bold;
        Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 170), new Vector2(1400, 170));

        TextMeshProUGUI detail = NewText("Detail", panel.transform, "", 38, TextAlignmentOptions.Center, new Color(0.9f, 0.86f, 0.78f));
        Place(detail.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(1200, 130));

        TextMeshProUGUI retryLabel;
        Button retry = NewButton("RetryButton", panel.transform, "RETRY", out retryLabel);
        Place((RectTransform)retry.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-220, -160), new Vector2(380, 90));

        TextMeshProUGUI menuLabel;
        Button menu = NewButton("MainMenuButton", panel.transform, "MAIN MENU", out menuLabel);
        Place((RectTransform)menu.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(220, -160), new Vector2(380, 90));

        Set(result, "panel", panel.gameObject);
        Set(result, "titleText", title);
        Set(result, "detailText", detail);
        Set(result, "retryButton", retry);
        Set(result, "mainMenuButton", menu);

        panel.gameObject.SetActive(false);
    }

    // ================= 小工具 =================

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        // 用 Directory 建立，若已有對應的 .meta（例如隊友建的 Data.meta）就會沿用原本的 GUID
        Directory.CreateDirectory(path);
        AssetDatabase.ImportAsset(path);
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.Refresh();
    }

    private static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");
        return (RectTransform)go.transform;
    }

    private static Image NewImage(string name, Transform parent, Color color)
    {
        RectTransform rt = NewUI(name, parent);
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        return img;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, TextAlignmentOptions align, Color color)
    {
        RectTransform rt = NewUI(name, parent);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.alignment = align;
        t.color = color;
        t.raycastTarget = false;
        return t;
    }

    private static Button NewButton(string name, Transform parent, string label, out TextMeshProUGUI labelText)
    {
        Image img = NewImage(name, parent, Sand);
        Sprite rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (rounded != null)
        {
            img.sprite = rounded;
            img.type = Image.Type.Sliced;
        }

        Button btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        labelText = NewText("Label", img.transform, label, 32, TextAlignmentOptions.Center, DarkText);
        labelText.fontStyle = FontStyles.Bold;
        Stretch(labelText.rectTransform);
        return btn;
    }

    private static void Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    // 上緣對齊、左右撐滿（留 sideMargin 的邊）
    private static void PlaceTop(RectTransform rt, float y, float height, float sideMargin)
    {
        Place(rt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, y), new Vector2(-sideMargin * 2f, height));
    }

    private static void Stretch(RectTransform rt, float margin = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(margin, margin);
        rt.offsetMax = new Vector2(-margin, -margin);
    }

    // 設定 [SerializeField] private 欄位
    private static void Set(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null)
        {
            Debug.LogError("組員E 工具：" + target.GetType().Name + " 找不到欄位 " + field);
            return;
        }
        p.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetList<T>(Object target, string field, List<T> values) where T : Object
    {
        var so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null)
        {
            Debug.LogError("組員E 工具：" + target.GetType().Name + " 找不到欄位 " + field);
            return;
        }
        p.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++)
        {
            p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
