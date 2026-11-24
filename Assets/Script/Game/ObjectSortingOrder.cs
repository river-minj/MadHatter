
using UnityEngine;

/// <summary>
/// [LEGACY] 스크립트 기반 Y-Sort 구현 (현재 미사용)
/// - 방식: transform.position.y로 sortingOrder를 매 프레임 계산
/// - 교체 사유: 타일맵 오브젝트(나무/표지판)는 스크립트를 붙일 수 없어 정렬 규칙이 이원화됨
///             → 엔진 축 정렬(Transparency Sort Axis + Sorting Group)로 통일
/// - 교체일: 2026-11-24
/// </summary>
[System.Obsolete("엔진 축 정렬(Transparency Sort Axis + SortingGroup)로 대체됨. 학습 기록용 레거시.")]
public class ObjectSortingOrder : MonoBehaviour
{
	[SerializeField] private Renderer _renderer;
	[SerializeField] private int _sortPrecision = 100; // Y값에 곱할 정밀도
	[SerializeField] private int _sortOffset = 0;      // 같은 Y에서 미세 조정용

	private void Awake()
	{
		if (_renderer == null)
			_renderer = GetComponent<Renderer>();
	}

	private void LateUpdate()
	{
		if (_renderer == null)
			return;

		// Y가 낮을수록 sortingOrder가 높아짐 → 앞에 그려짐
		_renderer.sortingOrder = -(int)(transform.position.y * _sortPrecision) + _sortOffset;
	}
}
