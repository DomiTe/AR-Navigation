using UnityEngine;
using UnityEditor;

// Runs once after Unity compiles — sets the HTW icon as the app icon for Android and iOS
[InitializeOnLoad]
public class SetAppIcon
{
    static SetAppIcon()
    {
        Texture2D icon = Resources.Load<Texture2D>("HTW_Icon");
        if (icon == null)
        {
            Debug.LogWarning("HTW_Icon not found in Resources folder.");
            return;
        }

        // Android
        SetIconsForTarget(BuildTargetGroup.Android, icon);

        // iOS
        SetIconsForTarget(BuildTargetGroup.iOS, icon);

        Debug.Log("HTW app icon set successfully.");
    }

    static void SetIconsForTarget(BuildTargetGroup group, Texture2D icon)
    {
        int[] sizes = PlayerSettings.GetIconSizesForTargetGroup(group);
        if (sizes == null || sizes.Length == 0) return;

        Texture2D[] icons = new Texture2D[sizes.Length];
        for (int i = 0; i < icons.Length; i++)
            icons[i] = icon;

        PlayerSettings.SetIconsForTargetGroup(group, icons);
    }
}
