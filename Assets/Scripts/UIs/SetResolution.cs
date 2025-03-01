using UnityEngine;

public class SetResolution : MonoBehaviour
{
    void Start()
    {
        // Đặt độ phân giải (Width, Height, Windowed mode)
        Screen.SetResolution(696, 373, false); // False = chế độ cửa sổ
        Debug.Log("Resolution set to 650x323");
    }
}
