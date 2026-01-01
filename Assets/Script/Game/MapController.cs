
using UnityEngine;

public class MapController : MonoBehaviour
{
	[SerializeField] private CameraController cameraController;
	[SerializeField] private MapBounds mapBounds;

	private void Start()
	{
		ApplyMapBounds();
	}

	private void ApplyMapBounds()
	{
		if (cameraController == null || mapBounds == null)
		{
			Debug.LogWarning("CameraController or MapBounds is not assigned.");
			return;
		}

		Vector2 minBounds = mapBounds.GetMinBounds();
		Vector2 maxBounds = mapBounds.GetMaxBounds();
		cameraController.SetBounds(minBounds, maxBounds);
	}
}
