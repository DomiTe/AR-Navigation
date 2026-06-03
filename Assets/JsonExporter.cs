using System.IO;
using System.Text;
using UnityEngine;

public class JsonExporter : MonoBehaviour
{
    [ContextMenu("Jetzt Exportieren")]
    public void Exportieren()
    {
        StringBuilder json = new StringBuilder();
        json.AppendLine("{");

        // Sucht alle Objekte in der Szene, die mit "Room_" anfangen
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        System.Collections.Generic.List<GameObject> roomObjects = new System.Collections.Generic.List<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (go.name.StartsWith("Room_"))
            {
                roomObjects.Add(go);
            }
        }

        // Sortiert die Räume sauber von 101 bis 175
        roomObjects.Sort((a, b) => a.name.CompareTo(b.name));

        for (int i = 0; i < roomObjects.Count; i++)
        {
            GameObject room = roomObjects[i];

            // Holt die echten WELT-KOORDINATEN (World Space)
            string xStr = room.transform.position.x.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            string zStr = room.transform.position.z.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            json.Append($"  \"{room.name.Trim()}\": {{ \"x\": {xStr}, \"z\": {zStr} }}");

            if (i < roomObjects.Count - 1) json.AppendLine(",");
            else json.AppendLine("");
        }

        json.AppendLine("}");

        string path = Path.Combine(Application.dataPath, "rooms.json");
        File.WriteAllText(path, json.ToString());

        Debug.Log("JSON mit echten WELT-KOORDINATEN erstellt: " + path);
    }
}