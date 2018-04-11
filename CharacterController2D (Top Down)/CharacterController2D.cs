using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour {

    List<Collider2D> coll = null;//List of all colliders attached to the same Rigidbody (ignoring triggers)
    [HideInInspector] public Vector2 velocity;

    ContactFilter2D contactFilter;
    int maxCollisions = 16;//Maximum amount of collisions at one time

    const float minMoveDistance = 0.001f;//It won't check for collisions if the Gameobject is stationary
    const float shellRadius = 0.01f;//Collider padding so it doesn't get stuck

    RaycastHit2D[] hitArray;//Array for collisions
    List<RaycastHit2D> hitList;//Filtered elements from the array get copied into the list

	// Use this for initialization
	void Start ()
    {
        hitArray = new RaycastHit2D[maxCollisions];
        hitList = new List<RaycastHit2D>(maxCollisions);
        coll = new List<Collider2D>();

        #region add colliders to the list
        coll.AddRange(GetComponents<Collider2D>());//Adds all colliders on the gameobject

        for (int i = 0; i < transform.childCount; i++)//Adds all colliders on child gameobjects
        {
            coll.AddRange(transform.GetChild(i).GetComponents<Collider2D>());
        }

        for (int i = 0; i < coll.Count; i++)//Removes triggers
        {
            if (coll[i].isTrigger)
                coll.Remove(coll[i]);
        }
        #endregion


        contactFilter.useTriggers = false;//Set the filter to ignore triggers
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));//Takes settings from the collision matrix in "Project Settings/Physics 2D"
        contactFilter.useLayerMask = true;
    }

    //This function is called every fixed framerate frame
    void FixedUpdate ()
    {
        Vector2 deltaPos = velocity * Time.fixedDeltaTime;

        MovePosition(deltaPos);
	}

    //Called when you first add the component to the gameobject or reset it's values (inspector only)
    void Reset()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
    }

    public void MovePosition(Vector2 move)
    {
        float distance = move.magnitude;

        if(distance > minMoveDistance)//Only checks for collisions if the gameobject is moving
        {
            hitList.Clear();

            for (int i = 0; i < coll.Count; i++)//Casts all of the colliders attached to the rigidbody in the direction of the velocity
            {
                int count = coll[i].Cast(move, contactFilter, hitArray, distance + shellRadius, true);

                for (int j = 0; j < count; j++)
                {
                    hitList.Add(hitArray[j]);//Adds results into the list
                }
            }

            for (int i = 0; i < hitList.Count; i++)
            {
                float modifiedDistance = hitList[i].distance - shellRadius;
                if (distance > modifiedDistance)//Blocks movement if delta velocity is greater than the distance to the collision point
                {
                    distance = modifiedDistance;
                }
            }
        }

        transform.position = (Vector2) transform.position + move.normalized * distance;//Moves the gameobject
    }
}
