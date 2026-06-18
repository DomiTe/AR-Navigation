using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniNav.Core;
using System.IO;
using Newtonsoft.Json;

public class InterfaceController : MonoBehaviour
{
    public Button B_StartNav;
    public Toggle T_Elevator; 
    public TMP_Text RouteText;
    public TMP_Dropdown DestinationInput;

    public PathController PathManager;

    // Beispiel-Array
    //public string[] destinations =
    //{
    //    "Raum A101",
    //    "Labor",
    //    "Cafeteria",
    //    "Ausgang"
    //};

    //private Dictionary<string, Vector3> demoDestinations = new Dictionary<string, Vector3>()
    //{
    //    { "Room_101_Door", new Vector3(-12.058f, 0f, -44.09f) }, // relative to Floor/
    //    { "Room_125_Door", new Vector3(11.37f, 0f, 22.26f) },
    //    { "Room_165_Door", new Vector3(-19.13f, 0f, -26.76f) }
    //};

    private Dictionary<string, Vector3> loadedDestinations = new Dictionary<string, Vector3>();
    private class CoordinateData {
        public float x {  get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start");
        RouteText.text = "";
        LoadDestinationsFromJson();
        FillDropdown();
        B_StartNav.onClick.AddListener(ShowRouteText);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ShowRouteText() {
        Debug.Log("Step 1: Button clicked.");

        // Check the Elevator Toggle
        if (T_Elevator == null) Debug.LogError("CRASH POINT: Elevator Toggle slot is empty in Inspector!");
        bool useElevator = T_Elevator.isOn;
        Debug.Log("Step 2: Elevator read successfully.");

        // Check the Dropdown
        if (DestinationInput == null) Debug.LogError("CRASH POINT: Dropdown slot is empty in Inspector!");
        if (DestinationInput.captionText == null) Debug.LogError("CRASH POINT: Dropdown is missing its caption text component!");
        string destination = DestinationInput.options[DestinationInput.value].text;
        Debug.Log("Step 3: Destination read successfully. Target is: " + destination);

        // Check the Route Text UI
        if (RouteText == null) {
            Debug.LogWarning("Route Text slot is empty, skipping text update.");
        }
        else {
            RouteText.text = "Routing to " + destination;
            Debug.Log("Step 4: UI Text updated successfully.");
        }

        Debug.Log("Step 5: Handing off to CalculateRoute...");
        CalculatingRoute(useElevator, destination);
    }

    public void LoadDestinationsFromJson() {
        TextAsset jsonFile = Resources.Load<TextAsset>("rooms");

        if (jsonFile != null) {
            // Get the text from the loaded asset
            string jsonText = jsonFile.text;
            var parsedData = JsonConvert.DeserializeObject<Dictionary<string, CoordinateData>>(jsonText);

            foreach (var kvp in parsedData) {
                Vector3 targetCoords = new Vector3(kvp.Value.x, kvp.Value.y, kvp.Value.z);
                loadedDestinations.Add(kvp.Key, targetCoords);
            }
        }
        else {
            Debug.LogError("Failed to load rooms.json from Resources folder!");
        }
    }

    public void CalculatingRoute(bool useElevator, string destination) {
        Debug.Log("Step 6: Entered CalculatingRoute.");

        if (PathManager == null) {
            Debug.LogError("CRASH POINT: PathManager slot is EMPTY in the InterfaceController Inspector!");
            return;
        }

        // Use demoDestinations for the lookup
        if (loadedDestinations.TryGetValue(destination, out Vector3 targetCoords)) {
            Debug.Log($"Step 7: Route Found in Dictionary at {targetCoords}");
            PathManager.SetTarget(targetCoords);
            Debug.Log("Step 8: Target successfully handed to the PathController.");
        }
        else {
            Debug.LogError($"CRASH POINT: Dictionary Lookup Failed! '{destination}' does not exist in rooms.");
        }
    }

    void FillDropdown() {
        DestinationInput.ClearOptions();
        List<string> options = new List<string>(loadedDestinations.Keys);
        DestinationInput.AddOptions(options);
    }
}
