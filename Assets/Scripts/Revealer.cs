using UnityEngine;

public class Revealer : MonoBehaviour
{
	public int sight;

	private NavMeshAgent _agent;

	private void Start()
	{
		_agent = GetComponent<NavMeshAgent>();
	}

	public void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit info;
			if (Physics.Raycast(ray, out info, 1000))
			{
				_agent.SetDestination(info.point);
			}
		}
	}
}
