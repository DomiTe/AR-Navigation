using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniNav.Core;
using Newtonsoft.Json;

public class InterfaceController : MonoBehaviour
{
    public Button       B_StartNav;
    public Toggle       T_Elevator;
    public TMP_Text     RouteText;
    public TMP_Dropdown DestinationInput;
    public TMP_Dropdown StartLocationDropdown;
    public PathController PathManager;

    // HTW green palette
    private static readonly Color GREEN_MAIN   = new Color(0.46f, 0.72f, 0.17f);
    private static readonly Color GREEN_DARK   = new Color(0.22f, 0.40f, 0.06f);
    private static readonly Color GREEN_LIGHT  = new Color(0.68f, 0.88f, 0.35f);
    private static readonly Color BG_DARK      = new Color(0.07f, 0.09f, 0.07f, 0.96f);
    private static readonly Color CARD_BG      = new Color(0.10f, 0.14f, 0.10f, 1.00f);
    private static readonly Color TEXT_LIGHT   = new Color(0.92f, 0.96f, 0.90f);
    private static readonly Color TEXT_MUTED   = new Color(0.55f, 0.72f, 0.40f);

    private Dictionary<string, Vector3> loadedDestinations = new Dictionary<string, Vector3>();

    private class CoordinateData {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }

    void Start() {
        Debug.Log("Start");
        RouteText.text = "";
        LoadDestinationsFromJson();
        FillDropdown();
        B_StartNav.onClick.AddListener(ShowRouteText);
        FixCanvasScalerForDevice();
    }

    // CanvasScaler dynamisch setzen damit Menü auf Tablet und Phone korrekt skaliert
    private void FixCanvasScalerForDevice() {
        Canvas menuCanvas = B_StartNav != null ? B_StartNav.GetComponentInParent<Canvas>() : FindObjectOfType<Canvas>();
        if (menuCanvas == null) return;
        CanvasScaler scaler = menuCanvas.GetComponent<CanvasScaler>();
        if (scaler == null) return;
        float refW = 1080f;
        float refH = refW * ((float)Screen.height / Screen.width);
        scaler.referenceResolution = new Vector2(refW, refH);
        scaler.matchWidthOrHeight  = 0f;
    }

    void Update() { }

    public void ShowRouteText() {
        Debug.Log("Step 1: Button clicked.");

        if (T_Elevator == null) Debug.LogError("CRASH POINT: Elevator Toggle slot is empty in Inspector!");
        bool useElevator = T_Elevator.isOn;
        Debug.Log("Step 2: Elevator read successfully.");

        if (StartLocationDropdown == null) Debug.LogError("CRASH POINT: Start Location Dropdown slot is empty in Inspector!");
        if (StartLocationDropdown.captionText == null) Debug.LogError("CRASH POINT: Start Location Dropdown is missing its caption text component!");
        string start = StartLocationDropdown.options[StartLocationDropdown.value].text;
        Debug.Log("Step 3: Start: " + start);

        if (DestinationInput == null) Debug.LogError("CRASH POINT: Dropdown slot is empty in Inspector!");
        if (DestinationInput.captionText == null) Debug.LogError("CRASH POINT: Dropdown is missing its caption text component!");
        string destination = DestinationInput.options[DestinationInput.value].text;
        Debug.Log("Step 4: Destination: " + destination);

        if (RouteText != null) {
            RouteText.text = destination;
            Debug.Log("Step 5: UI updated.");
        }

        Debug.Log("Step 6: Handing off to CalculateRoute...");
        CalculatingRoute(useElevator, start, destination);
    }

    public void LoadDestinationsFromJson() {
        TextAsset jsonFile = Resources.Load<TextAsset>("rooms");
        if (jsonFile != null) {
            var parsedData = JsonConvert.DeserializeObject<Dictionary<string, CoordinateData>>(jsonFile.text);
            foreach (var kvp in parsedData)
                loadedDestinations.Add(kvp.Key, new Vector3(kvp.Value.x, kvp.Value.y, kvp.Value.z));
        } else {
            Debug.LogError("Failed to load rooms.json from Resources folder!");
        }
    }

    public void CalculatingRoute(bool useElevator, string start, string destination) {
        Debug.Log("Step 7: Entered CalculatingRoute.");
        if (PathManager == null) { Debug.LogError("CRASH POINT: PathManager slot is EMPTY!"); return; }
        if (!loadedDestinations.TryGetValue(destination, out Vector3 targetCoords)) { Debug.LogError($"Destination '{destination}' not found."); return; }
        if (!loadedDestinations.TryGetValue(start, out Vector3 startCoords))        { Debug.LogError($"Start '{start}' not found."); return; }
        Debug.Log($"Routing from {start} to {destination}");
        PathManager.SetTarget(targetCoords, destination);
        PathManager.SetStart(startCoords);
    }

    void FillDropdown() {
        DestinationInput.ClearOptions();
        StartLocationDropdown.ClearOptions();
        List<string> options = new List<string>(loadedDestinations.Keys);
        DestinationInput.AddOptions(options);
        StartLocationDropdown.AddOptions(options);
    }

    // ---------------------------------------------------------------
    // Menu theming — runs automatically at Start()

    private void ApplyMenuTheme() {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        InsertBackground(canvas.transform);
        InsertHeader(canvas.transform);

        // Style existing UI elements
        StyleDropdowns();
        StyleButton();
        StyleToggle();

        if (RouteText != null) {
            RouteText.color     = GREEN_MAIN;
            RouteText.fontStyle = FontStyles.Bold;
        }
    }

    private void InsertBackground(Transform canvasT) {
        GameObject bg = new GameObject("MenuBG");
        bg.transform.SetParent(canvasT, false);
        bg.transform.SetAsFirstSibling();
        RectTransform rt = bg.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        bg.AddComponent<Image>().color = BG_DARK;
    }

    private void InsertHeader(Transform canvasT) {
        // Header bar
        GameObject header = new GameObject("MenuHeader");
        header.transform.SetParent(canvasT, false);
        header.transform.SetSiblingIndex(1);
        RectTransform hRt = header.AddComponent<RectTransform>();
        hRt.anchorMin = new Vector2(0f, 1f);
        hRt.anchorMax = new Vector2(1f, 1f);
        hRt.pivot     = new Vector2(0.5f, 1f);
        hRt.offsetMin = new Vector2(0f, -155f);
        hRt.offsetMax = Vector2.zero;
        header.AddComponent<Image>().color = GREEN_DARK;

        // Title text
        GameObject titleGO = new GameObject("HeaderTitle");
        titleGO.transform.SetParent(header.transform, false);
        RectTransform tRt = titleGO.AddComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.offsetMin = new Vector2(20f, 8f);
        tRt.offsetMax = new Vector2(-20f, -8f);
        TextMeshProUGUI title = titleGO.AddComponent<TextMeshProUGUI>();
        title.text      = "HTW Navigation";
        title.fontSize  = 50;
        title.fontStyle = FontStyles.Bold;
        title.color     = Color.white;
        title.alignment = TextAlignmentOptions.Center;

        // Green accent line
        GameObject accent = new GameObject("AccentLine");
        accent.transform.SetParent(canvasT, false);
        accent.transform.SetSiblingIndex(2);
        RectTransform aRt = accent.AddComponent<RectTransform>();
        aRt.anchorMin = new Vector2(0f, 1f);
        aRt.anchorMax = new Vector2(1f, 1f);
        aRt.pivot     = new Vector2(0.5f, 1f);
        aRt.offsetMin = new Vector2(0f, -160f);
        aRt.offsetMax = new Vector2(0f, -155f);
        accent.AddComponent<Image>().color = GREEN_MAIN;
    }

    private void StyleDropdowns() {
        TMP_Dropdown[] dropdowns = FindObjectsOfType<TMP_Dropdown>();
        foreach (var dd in dropdowns) {
            Image bg = dd.GetComponent<Image>();
            if (bg != null) bg.color = CARD_BG;

            if (dd.captionText != null) {
                dd.captionText.color    = TEXT_LIGHT;
                dd.captionText.fontSize = 34;
            }

            Transform arrow = dd.transform.Find("Arrow");
            if (arrow != null) {
                Image arrowImg = arrow.GetComponent<Image>();
                if (arrowImg != null) arrowImg.color = GREEN_MAIN;
            }

            if (dd.template != null) {
                Image tBg = dd.template.GetComponent<Image>();
                if (tBg != null) tBg.color = CARD_BG;
                foreach (var t in dd.template.GetComponentsInChildren<TMP_Text>(true))
                    t.color = TEXT_LIGHT;
            }
        }

        // Labels above dropdowns
        if (StartLocationDropdown != null) AddFieldLabel(StartLocationDropdown.transform, "Startpunkt");
        if (DestinationInput      != null) AddFieldLabel(DestinationInput.transform,      "Ziel");
    }

    private void StyleButton() {
        if (B_StartNav == null) return;
        Image img = B_StartNav.GetComponent<Image>();
        if (img != null) img.color = GREEN_MAIN;

        TMP_Text label = B_StartNav.GetComponentInChildren<TMP_Text>();
        if (label != null) {
            label.text      = "Navigation starten";
            label.color     = Color.white;
            label.fontStyle = FontStyles.Bold;
            label.fontSize  = 38;
        }

        ColorBlock cb    = B_StartNav.colors;
        cb.normalColor   = GREEN_MAIN;
        cb.highlightedColor = GREEN_LIGHT;
        cb.pressedColor  = GREEN_DARK;
        B_StartNav.colors = cb;
    }

    private void StyleToggle() {
        if (T_Elevator == null) return;
        ColorBlock cb    = T_Elevator.colors;
        cb.normalColor   = CARD_BG;
        cb.highlightedColor = GREEN_LIGHT;
        T_Elevator.colors = cb;

        TMP_Text label = T_Elevator.GetComponentInChildren<TMP_Text>();
        if (label != null) { label.color = TEXT_LIGHT; label.fontSize = 30; }
    }

    private void AddFieldLabel(Transform parent, string text) {
        if (parent.Find("FieldLabel") != null) return;
        GameObject go = new GameObject("FieldLabel");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.offsetMin = new Vector2(0f, 4f);
        rt.offsetMax = new Vector2(0f, 38f);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 26;
        tmp.color     = TEXT_MUTED;
        tmp.alignment = TextAlignmentOptions.Left;
    }
}
