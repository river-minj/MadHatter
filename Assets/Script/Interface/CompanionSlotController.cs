using TMPro;
using UnityEngine;

public class CompanionSlotController : MonoBehaviour
{
	[SerializeField]  private TextMeshProUGUI _nameText;

	public void SetData(CompanionData companion)
	{
		if(companion == null)
		{
			Debug.LogWarning("Companion data is null.");
			return;
		}

		if (_nameText != null)
		{
			_nameText.text = companion._companionName;
		}

		// Set up the UI elements based on the companion data
		// For example:
		// _nameText.text = companion.Name;
		// _iconImage.sprite = companion.Icon;
	}
}
