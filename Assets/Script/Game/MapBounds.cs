using UnityEngine;

/// <summary>
/// calculate map size only
/// </summary>
public class MapBounds : MonoBehaviour
{
	[SerializeField] private BoxCollider2D _boundsCollider;

	public Bounds _mapBound { get; private set; }

	private void Awake()
	{
		if(_boundsCollider == null)
		{
			_boundsCollider = GetComponent<BoxCollider2D>();
			if(_boundsCollider == null)
			{
				Debug.LogError("BoxCollider2D component is required for MapBounds.");
				return;
			}
		}

		_mapBound = _boundsCollider.bounds;
	}

	public Bounds GetBounds()
	{
		if(_boundsCollider == null)
			{
			Debug.LogError("BoxCollider2D component is required for MapBounds.");
			return new Bounds();
		}

		return _boundsCollider.bounds;
	}


#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (_boundsCollider == null) return;

		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_boundsCollider.bounds.center, _boundsCollider.bounds.size);
	}
#endif
}
