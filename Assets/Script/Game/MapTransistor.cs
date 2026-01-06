using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The trigger of map transition
/// e.g., door, portal
/// </summary>
public class MapTransistor : MonoBehaviour
{
	[SerializeField] private MapBounds _nextMapBounds;
	[SerializeField] private Vector3 _playerSpawnPosition;

	private bool triggered = false;


	private void Awake()
	{
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player") == false)
			return;

		 MapController _mapController;
		_mapController = FindObjectOfType<MapController>();

		if (_mapController == null)
		{
			Debug.LogWarning("MapController or next MapBounds is not assigned.");
			return;
		}
		
		_mapController.ChangeMap(_nextMapBounds, _playerSpawnPosition);
		
	}
}
