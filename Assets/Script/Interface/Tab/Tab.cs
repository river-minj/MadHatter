using UnityEngine;
using UnityEngine.UI;

public class Tab : MonoBehaviour
{

	[SerializeField] private Button _tabButton;
	[SerializeField] private TabPage _linkedPage;

	public Button TabButton => _tabButton;
	public TabPage LinkedPage => _linkedPage;
}

