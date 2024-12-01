using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
	public GameObject bullet = null;
	public GameObject OutSpawm = null;
	public int Ammo = 50;
	public int Rate = 5;
	public bool Shoot;
	public int R = 0;



	void Start () {
		Shoot = false;
	}
	
	void FixedUpdate()
	{
		if (Shoot == true && Ammo > 0)
		{
			Shot();
		}
	}



	void Shot () {
		R++;

		if(R == Rate*10)
		{
				GameObject newBullet = Instantiate(bullet, OutSpawm.transform.position , Quaternion.Euler(0, 0, 0)) as GameObject;
				Rigidbody rb = newBullet.GetComponent<Rigidbody>();
				rb.linearVelocity = OutSpawm.transform.TransformDirection(Vector3.forward * 20);
			    Ammo--;
			    R = 0;
		}
	}

}
