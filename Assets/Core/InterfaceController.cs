using System.Collections.Generic;
using System.Linq;
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
    public static string[] destinations =
    {
        "Raum 101", "Raum 102", "Raum 103", "Raum 104", "Raum 105", "Raum 106", "Raum 107", "Raum 108", "Raum 109", "Raum 110",
        "Raum 111", "Raum 112", "Raum 113", "Raum 114", "Raum 115", "Raum 116", "Raum 117", "Raum 118", "Raum 119", "Raum 120",
        "Raum 121", "Raum 122", "Raum 123", "Raum 124", "Raum 125", "Raum 126", "Raum 127", "Raum 128", "Raum 129", "Raum 130",
        "Raum 131", "Raum 132", "Raum 133", "Raum 134", "Raum 135", "Raum 136", "Raum 137", "Raum 138", "Raum 139", "Raum 140",
        "Raum 141", "Raum 142", "Raum 143", "Raum 144", "Raum 145", "Raum 146", "Raum 147", "Raum 148", "Raum 149", "Raum 150",
        "Raum 151", "Raum 152", "Raum 153", "Raum 154", "Raum 155", "Raum 156", "Raum 157", "Raum 158", "Raum 159", "Raum 160",
        "Raum 161", "Raum 162", "Raum 163", "Raum 164", "Raum 165", "Raum 166", "Raum 167", "Raum 168", "Raum 169", "Raum 170",
        "Raum 171", "Raum 172", "Raum 173", "Raum 174", "Raum 175"
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
