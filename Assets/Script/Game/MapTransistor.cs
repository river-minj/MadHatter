using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The trigger of map transition
/// e.g., door, portal
/// </summary>
public class MapTransistor : MonoBehaviour
{
	[SerializeField] private MapBounds _mextMapBounds;
	[SerializeField] private Vector3 _playerSpawnPosition;

	private MapController _mapController;

	private void Awake()
	{
		_mapController = FindObjectOfType<MapController>();
		if(_mapController == null)
		{
			Debug.LogError("MapController not found in the scene.");
		}
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
		
		_mapController.ChangeMap(_mextMapBounds, _playerSpawnPosition);
		
	}
}
