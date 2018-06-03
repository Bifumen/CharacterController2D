using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour {

    private class ColliderContainer
    {
        public Collider2D collider;
        public LayerMask layerMask;

        public ColliderContainer(Collider2D coll,LayerMask mask)
        {
            collider = coll;
            layerMask = mask;
        }
    }

    List<ColliderContainer> colliderList = null;//List of all colliders attached to the same Rigidbody (ignoring triggers)


    Vector2 targetVelocity;
    Vector2 currentVelocity;

    public Vector2 velocity
    {
        get
        {
            return currentVelocity;
        }
        set
        {
            targetVelocity = value;
        }
    }


    ContactFilter2D contactFilter;
    int maxCollisions = 16;//Maximum amount of collisions at one time

    const float minMoveDistance = 0.001f;//It won't check for collisions if the Gameobject is stationary
    const float shellRadius = 0.01f;//Collider padding so it doesn't get stuck
    const float overlapThreshold = -0.008f;//if the distance between two objects is less than this value the objects are overlapping 

    RaycastHit2D[] hitArray;//Array for collisions
    List<RaycastHit2D> hitList;//Filtered elements from the array get copied into the list

	// Use this for initialization
	void Start ()
    {
        hitArray = new RaycastHit2D[maxCollisions];
        hitList = new List<RaycastHit2D>(maxCollisions);
        colliderList = new List<ColliderContainer>();

        #region add colliders to the list
        List<Collider2D> coll = new List<Collider2D>();

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

        for (int i = 0; i < coll.Count; i++)//Adds the collider and the layer mask of it's gameobject
        {
            colliderList.Add(new ColliderContainer(coll[i], Physics2D.GetLayerCollisionMask(coll[i].gameObject.layer)));
            //Takes settings from the collision matrix in "Project Settings/Physics 2D"
        }

        #endregion

        contactFilter.useTriggers = false;//Set the filter to ignore triggers
        contactFilter.useLayerMask = true;
    }

    //This function is called every fixed framerate frame
    void FixedUpdate ()
    {
        Vector2 deltaPos;
        currentVelocity = targetVelocity;

        deltaPos = new Vector2(currentVelocity.x * Time.fixedDeltaTime, 0f);

        MovePosition(deltaPos);//Moving horizontally

        deltaPos = new Vector2(0f, currentVelocity.y * Time.fixedDeltaTime);

        MovePosition(deltaPos);//Moving vertically
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

            for (int i = 0; i < colliderList.Count; i++)//Casts all of the colliders attached to the rigidbody in the direction of the velocity
            {
                contactFilter.SetLayerMask(colliderList[i].layerMask);
                int count = colliderList[i].collider.Cast(move, contactFilter, hitArray, distance + shellRadius, true);

                for (int j = 0; j < count; j++)
                {
                    hitList.Add(hitArray[j]);//Adds results into the list
                }
            }

            for (int i = 0; i < hitList.Count; i++)
            {
                Vector2 currentNormal = hitList[i].normal;//A vector that's perpendicular to the object we are colliding with

                float projection = Vector2.Dot(currentVelocity, currentNormal);//The dot product of our velocity and the normal
                /*If the dot product is 0, then the two vectors are perpendicular
                 if it's 1 then the vectors are pointing in the same direction
                 if it's -1 then the vectors are pointing in the opposite directions*/
                if (projection < 0)
                {
                    currentVelocity = currentVelocity - projection * currentNormal;//If the dot product is less than 0 we subtract the velocity 
                }


                float modifiedDistance = hitList[i].distance - shellRadius;
                if (distance > modifiedDistance)
                {
                    if (modifiedDistance < overlapThreshold)
                    {
                        move = (Vector2)transform.position - hitList[i].point;//if objects are overlapping make them move away from each other
                        currentVelocity = move;
                    }
                    else
                    {
                        distance = modifiedDistance;//Blocks movement if delta velocity is greater than the distance to the collision point
                    }
                }
            }
        }

        
        transform.position = (Vector2) transform.position + move.normalized * distance;//Moves the gameobject
    }
}
