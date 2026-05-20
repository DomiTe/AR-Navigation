using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceController : MonoBehaviour
{
    public Button B_StartNav;
    public Toggle T_Elevator; 
    public TMP_Text RouteText;
    public TMP_Dropdown DestinationInput;

    // Beispiel-Array
    public string[] destinations =
    {
        "Raum A101",
        "Labor",
        "Cafeteria",
        "Ausgang"
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

    public void ShowRouteText()
    {
        Debug.Log("You have clicked the button!");
        bool useElevator = T_Elevator.isOn;
        string destination = DestinationInput.captionText.text;
        RouteText.text = "Calculating best route ...";
        CalculatingRoute(useElevator, destination);
    }

    public void CalculatingRoute(bool useElevator, string destination)
    {
        if (useElevator)
        {
            // Elevator selected 
            Debug.Log("Elevator selected to destination: "+ destination);
        }
        else
        {
            // Staires selcted
            Debug.Log("Staires selected to destination: "+ destination);
        }
    }

    void FillDropdown()
    {
        DestinationInput.ClearOptions();

        List<string> options =
            new List<string>(destinations);

        DestinationInput.AddOptions(options);
    }
}
