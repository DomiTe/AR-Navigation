using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniNav.Core;

public class InterfaceController : MonoBehaviour
{
    public Button B_StartNav;
    public Toggle T_Elevator; 
    public TMP_Text RouteText;
    public TMP_Dropdown DestinationInput;
    public TMP_Dropdown StartLocationDropdown; 

    public PathController PathManager;

    // Beispiel-Array
    //public string[] destinations =
    //{
    //    "Raum A101",
    //    "Labor",
    //    "Cafeteria",
    //    "Ausgang"
    //};

    private Dictionary<string, Vector3> demoDestinations = new Dictionary<string, Vector3>()
    {
        { "Room_101_Door", new Vector3(9.368f, 0f, 14.793f) }, //worldspace
        { "Room_125_Door", new Vector3(30.094f, 0f, 54.21f) },
        { "Room_165_Door", new Vector3(-19.13f, 0f, -26.76f) }
    };


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start");
        RouteText.text = "";
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

        // Check the Start Location Dropdown
        if (StartLocationDropdown == null) Debug.LogError("CRASH POINT: Start Location Dropdown slot is empty in Inspector!");
        if (StartLocationDropdown.captionText == null) Debug.LogError("CRASH POINT: Start Location Dropdown is missing its caption text component!");
        string start = StartLocationDropdown.options[StartLocationDropdown.value].text;
        Debug.Log("Step 3: Start Location read successfully. Start is: " + start);

        // Check the Destination Dropdown
        if (DestinationInput == null) Debug.LogError("CRASH POINT: Dropdown slot is empty in Inspector!");
        if (DestinationInput.captionText == null) Debug.LogError("CRASH POINT: Dropdown is missing its caption text component!");
        string destination = DestinationInput.options[DestinationInput.value].text;
        Debug.Log("Step 4: Destination read successfully. Target is: " + destination);

        // Check the Route Text UI
        if (RouteText == null) {
            Debug.LogWarning("Route Text slot is empty, skipping text update.");
        }
        else {
            RouteText.text = "Routing to " + destination;
            Debug.Log("Step 5: UI Text updated successfully.");
        }

        Debug.Log("Step 6: Handing off to CalculateRoute...");
        CalculatingRoute(useElevator, start, destination);
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

    public void CalculatingRoute(bool useElevator, string start, string destination) {
        Debug.Log("Step 7: Entered CalculatingRoute.");

        if (PathManager == null) {
            Debug.LogError("CRASH POINT: PathManager slot is EMPTY in the InterfaceController Inspector!");
            return;
        }

        // Use demoDestinations for the lookup
        if (loadedDestinations.TryGetValue(destination, out Vector3 targetCoords) && loadedDestinations.TryGetValue(start, out Vector3 startCoords)) {
            Debug.Log($"Step 8: Route Found in Dictionary at {targetCoords} and {startCoords}.");
            PathManager.SetTarget(targetCoords);
            Debug.Log("Step 9: Target successfully handed to the PathController.");
            PathManager.SetStart(startCoords);
            Debug.Log("Step 10: Start successfully handed to the PathController.");
        }
        else {
            Debug.LogError($"CRASH POINT: Dictionary Lookup Failed! '{destination}' or '{start}' does not exist in rooms.");
        }
    }

    void FillDropdown() {
        DestinationInput.ClearOptions();
        StartLocationDropdown.ClearOptions();
        List<string> options = new List<string>(loadedDestinations.Keys);
        DestinationInput.AddOptions(options);
        StartLocationDropdown.AddOptions(options);
    }
}
