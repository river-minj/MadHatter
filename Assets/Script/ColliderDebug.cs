using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class ColliderDebug : MonoBehaviour
{
	void Start()
	{
		CheckColliders();
	}

	void CheckColliders()
	{
		// Tilemap Collider 확인
		var tilemapCollider = GetComponent<TilemapCollider2D>();
		if (tilemapCollider != null)
		{
			Debug.Log("TilemapCollider2D 존재");
			Debug.Log($"   Used By Composite: {tilemapCollider.usedByComposite}");
		}
		else
		{
			Debug.LogError(" TilemapCollider2D 없음!");
		}

		// Composite Collider 확인
		var compositeCollider = GetComponent<CompositeCollider2D>();
		if (compositeCollider != null)
		{
			Debug.Log(" CompositeCollider2D 존재");
			Debug.Log($"   Geometry Type: {compositeCollider.geometryType}");
			Debug.Log($"   Point Count: {compositeCollider.pointCount}");

			// 포인트가 0이면 Collider 생성 안 된 것
			if (compositeCollider.pointCount == 0)
			{
				Debug.LogWarning("Composite Collider가 비어있음! 타일을 그려주세요.");
			}
		}
		else
		{
			Debug.LogError("CompositeCollider2D 없음!");
		}

		// Rigidbody 확인
		var rb = GetComponent<Rigidbody2D>();
		if (rb != null)
		{
			Debug.Log("Rigidbody2D 존재");
			Debug.Log($"   Body Type: {rb.bodyType}");

			if (rb.bodyType != RigidbodyType2D.Static)
			{
				Debug.LogWarning("Body Type이 Static이 아닙니다!");
			}
		}
		else
		{
			Debug.LogError("Rigidbody2D 없음!");
		}
	}

	void OnDrawGizmos()
	{
		// 강제로 Collider 경계 그리기
		var compositeCollider = GetComponent<CompositeCollider2D>();
		if (compositeCollider != null)
		{
			Gizmos.color = Color.red;

			for (int i = 0; i < compositeCollider.pathCount; i++)
			{
				Vector2[] path = new Vector2[compositeCollider.GetPathPointCount(i)];
				compositeCollider.GetPath(i, path);

				for (int j = 0; j < path.Length - 1; j++)
				{
					Gizmos.DrawLine(
						transform.TransformPoint(path[j]),
						transform.TransformPoint(path[j + 1])
					);
				}

				// 마지막과 첫 점 연결
				if (path.Length > 0)
				{
					Gizmos.DrawLine(
						transform.TransformPoint(path[path.Length - 1]),
						transform.TransformPoint(path[0])
					);
				}
			}
		}
	}
}