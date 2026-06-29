using UnityEngine;

/// <summary>
/// Debug on-screen: tampilkan nilai semua axis di layar game.
/// Pasang di GameObject manapun, Play, gerakkan analog kanan.
/// Lihat axis mana yang berubah. HAPUS setelah selesai.
/// </summary>
public class GamepadAxisDebug : MonoBehaviour
{
    private float[] axisValues = new float[11]; // axis 1-10

    void Update()
    {
        // Baca semua axis yang sudah didefinisikan di Input Manager
        for (int i = 0; i < axisValues.Length; i++)
            axisValues[i] = 0f;

        // Axis yang sudah ada di Input Manager
        Try("Horizontal", 1);
        Try("Vertical", 2);
        Try("RightStickX", 4);
        Try("RightStickY", 5);
        Try("TestAxis3", 3);
        Try("TestAxis6", 6);
        Try("TestAxis8", 8);
    }

    void Try(string name, int slot)
    {
        try { axisValues[slot] = Input.GetAxisRaw(name); }
        catch { }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 22;
        style.fontStyle = FontStyle.Bold;

        GUIStyle header = new GUIStyle(style);
        header.fontSize = 26;

        float y = 10;
        GUI.Label(new Rect(10, y, 600, 35), "=== GAMEPAD AXIS DEBUG ===", header);
        y += 35;
        GUI.Label(new Rect(10, y, 600, 30), "Gerakkan HANYA analog kanan, lihat mana yang berubah!", style);
        y += 40;

        string[] names = { "", "Horizontal(1)", "Vertical(2)", "TestAxis3(3)", "RightStickX(4)", "RightStickY(5)", "TestAxis6(6)", "", "TestAxis8(8)" };

        for (int i = 1; i <= 8; i++)
        {
            if (i == 7) continue; // skip
            string label = (i < names.Length && names[i] != "") ? names[i] : $"Axis {i}";
            float val = axisValues[i];

            // Highlight jika aktif
            if (Mathf.Abs(val) > 0.1f)
                style.normal.textColor = Color.yellow;
            else
                style.normal.textColor = Color.white;

            GUI.Label(new Rect(10, y, 600, 30), $"  {label}: {val:F3}", style);
            y += 28;
        }

        y += 20;
        style.normal.textColor = Color.cyan;
        GUI.Label(new Rect(10, y, 800, 30), "Axis yang KUNING saat gerak analog kanan = axis yang benar", style);
    }
}
