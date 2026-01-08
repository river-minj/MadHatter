using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The trigger of map transition
/// e.g., door, portal
/// </summary>
public class MapTransistor : MonoBehaviour
{
	[SerializeField] private GameObject _nextMapPrefab;
	[SerializeField] private Vector3 _playerSpawnPosition;

	private MapController _mapController;

	private void Awake()
	{
		_mapController = FindObjectOfType<MapController>();
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player") == false)
			return;

		if (_mapController == null)
		{
			Debug.LogWarning("MapController or next MapBounds is not assigned.");
			return;
		}
		
		//_mapController.ChangeMap(_nextMapBounds, _playerSpawnPosition);
		
	}
}
