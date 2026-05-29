using System.IO;
using System.Text;
using UnityEngine;

public class JsonExporter : MonoBehaviour
{
    // Dadurch bekommen wir eine Klick-Funktion im Inspector!
    [ContextMenu("Jetzt Exportieren")]
    public void Exportieren()
    {
        StringBuilder json = new StringBuilder();
        json.AppendLine("{");

        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);

            string xStr = child.position.x.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            string zStr = child.position.z.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            json.Append($"  \"{child.name}\": {{ \"x\": {xStr}, \"z\": {zStr} }}");

            if (i < childCount - 1) json.AppendLine(",");
            else json.AppendLine("");
        }

        json.AppendLine("}");

        string path = Path.Combine(Application.dataPath, "rooms.json");
        File.WriteAllText(path, json.ToString());

        Debug.Log("JSON erfolgreich exportiert unter: " + path);
    }
}