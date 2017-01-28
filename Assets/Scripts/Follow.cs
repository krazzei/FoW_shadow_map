using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour
{
	public Transform target;
	public Vector3 offset;

	private Transform _transform;

	private void Awake()
	{
		_transform = transform;
	}

	private void Update()
	{
		_transform.position = target.position + offset;
	}
}
