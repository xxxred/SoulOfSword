using UnityEngine;

/// <summary>
/// Attach to a game object to make its rotation always lag behind its parent as the parent rotates.
/// </summary>

[AddComponentMenu("NGUI/Examples/Lag Rotation")]
public class LagRotation : MonoBehaviour
{
	public int level = 0;
	public float speed = 10f;
	
	Transform mTrans;
	Quaternion mRelative;
	Quaternion mAbsolute;
	
	void Start()
	{
		mTrans = transform;
		mRelative = mTrans.localRotation;
		mAbsolute = mTrans.rotation;
	}

	void LateUpdate()
	{
		Transform parent = mTrans.parent;
		
		if (parent != null)
		{
			mAbsolute = Quaternion.Slerp(mAbsolute, parent.rotation * mRelative, Time.deltaTime * speed);
			mTrans.rotation = mAbsolute;
		}
	}
}