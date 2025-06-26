using TMPro;
using UnityEngine;

public static class Utils
{
    public static bool IsAnyInputEmpty(params TMP_InputField[] inputFields)
    {
        foreach (var field in inputFields)
        {
            if (string.IsNullOrEmpty(field.text)) return true;
        }
        return false;
    }
}
