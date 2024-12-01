using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IBrain_ToolCharacter : EditorWindow
{
	bool CloseAfterCreateCharacter = false;
	//int m_AxlesCount = 2;
	float m_Mass = 1;
	float m_AxleStep = 2;
	float m_AxleWidth = 2;
	float m_AxleShift = -0.5f;
	public GameObject obj = null;
	public RuntimeAnimatorController Ani = null;

	[MenuItem ("IBRAIN/Create Character")]
	public static void  ShowWindow2 ()
    {
		GetWindow(typeof(IBrain_ToolCharacter));
	}

	void OnGUI ()
    {
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		obj = (GameObject)EditorGUI.ObjectField(new Rect(3, 3, position.width - 6, 20), "Select 3D Character", obj, typeof(GameObject));
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		EditorGUILayout.LabelField (" Select only 3D model file");
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		Ani = (RuntimeAnimatorController)EditorGUI.ObjectField(new Rect(3, 60, position.width - 6, 20), "Select Animator", Ani, typeof(RuntimeAnimatorController));
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		EditorGUILayout.LabelField (" Select only Animator file");
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		m_Mass = EditorGUILayout.FloatField ("Mass: ", m_Mass);
		EditorGUILayout.Space();

		CloseAfterCreateCharacter = EditorGUILayout.Toggle("Close After Create", CloseAfterCreateCharacter);
		EditorGUILayout.Space();


		if (GUILayout.Button("Create Character")) 
        {
			CreateCharacter ();
		}
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		if (GUILayout.Button("Close")) 
		{
			this.Close();
		}


	}

	void CreateCharacter()
	{
		if(obj !=null)
		{	
		var root = new GameObject (obj.name);
		root.AddComponent<UnityEngine.AI.NavMeshAgent>();

		var rootBody = root.AddComponent<Rigidbody> ();
		rootBody.mass = m_Mass;
		rootBody.useGravity = true;


		var body = Instantiate (obj.gameObject);
		body.transform.parent = root.transform;
		body.transform.localScale = new Vector3(1, 1, 1);

		var col = root.AddComponent<CapsuleCollider> ();
			col.isTrigger = true;

		var scr = root.AddComponent<IBrain>();
			scr.mode = IBrain.Mode.Random;



		var ami = root.AddComponent<Animator> ();

		Animator  FindAvatar = body.GetComponentInChildren(typeof(Animator )) as Animator ;
		Debug.Log ("Source Avatar: " + FindAvatar.avatar.name);
		FindAvatar.enabled = false;
		ami.avatar = FindAvatar.avatar;

			if(Ani !=null)
			{	
				ami.runtimeAnimatorController = Ani;
				//Destroy(body.GetComponent(typeof(Animator))as Animator);
			}



		if(CloseAfterCreateCharacter == true)
			this.Close();

	    }




	}
}