using UnityEngine;

/// <summary>
/// calculate map size only
/// </summary>
public class MapBounds : MonoBehaviour
{
	[SerializeField] private BoxCollider2D boundsCollider;

	public Bounds mapBound { get; private set; }

	private void Awake()
	{
		if(boundsCollider == null)
		{
			boundsCollider = GetComponent<BoxCollider2D>();
			if(boundsCollider == null)
			{
				Debug.LogError("BoxCollider2D component is required for MapBounds.");
				return;
			}
		}

		mapBound = boundsCollider.bounds;
	}

	public Vector2 GetMinBounds()
	{
		return mapBound.min;
	}
	public Vector2 GetMaxBounds()
	{
		return mapBound.max;
	}
}
