using UnityEngine;
using System.Collections;

public class ThirdPersonCamera : MonoBehaviour
{
	public Transform Target;		
	public float distanceAway;			// distance from the back of the craft
	public float distanceUp;			// distance above the craft
	public float smooth;				// how smooth the camera movement is
	private Vector3 targetPosition;		// the position the camera is trying to be in
	
	Transform follow;
	

	
	void LateUpdate ()
	{
		if(Target)
			
		{	
			
		targetPosition = Target.position + Vector3.up * distanceUp - Target.forward * distanceAway;
		transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smooth);
		transform.LookAt(Target);

		}
	}
}
