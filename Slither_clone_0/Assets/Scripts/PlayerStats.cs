using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lengthText;

    private void OnEnable()
    {
        PlayerLength.ChangedLengthEvent += ChangedLengthText;
    }

    private void OnDisable()
    {
        PlayerLength.ChangedLengthEvent -= ChangedLengthText;

    }

    private void ChangedLengthText(ushort length)
    {
        _lengthText.text = length.ToString();
    }
}
