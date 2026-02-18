using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
