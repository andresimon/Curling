using UnityEngine;
using System.Collections;

public class ThrowCurling : MonoBehaviour {
	
	public float mForce = 0.0f;

    private bool crossedHogLine;

    void OnStart()
    {
        crossedHogLine = false;
    }

	public void Throw () 
    {
		this.GetComponent<Rigidbody>().AddForce(mForce,0,0,ForceMode.Force);
	}

	void OnCollisionEnter(Collision collision) {
		if(collision.rigidbody != null)
		{
			collision.rigidbody.useGravity = true;
			this.GetComponent<Rigidbody>().useGravity = true;
		}
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.name == "HogLine")
        {
            crossedHogLine = true;
        }
    }

    public bool GetCrossedLine() { return crossedHogLine; }

}
