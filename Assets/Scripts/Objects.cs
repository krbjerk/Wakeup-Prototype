using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Entities : MonoBehaviour
{
    [System.Serializable]

    public class Entity
    {
        public enum ObjectType{Box, Ladder};
        public ObjectType Type;
        public int Health;
        public float Weight;
    }
    public Entity Instance;

    public float explosionForce = 5f;
    public float explosionRadius = 2f;

    public void TakeDamage(int Damage, Vector3 Direction)
    {
        Instance.Health -= Damage;
        if (Instance.Health <= 0)
        {
            Destroy(Direction);
        }
    }

    public void Destroy(Vector3 Direction)
    {

        foreach (Transform child in transform)
        {

            child.SetParent(null);

            if (child.GetComponent<Collider>() == null)
            {
                if (HasNegativeScale(child))
                {
                    var meshCollider = child.gameObject.AddComponent<MeshCollider>();
                    meshCollider.convex = true;
                }
                else
                {
                    child.gameObject.AddComponent<BoxCollider>();
                }
            }

            if (child.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = child.gameObject.AddComponent<Rigidbody>();
                rb.mass = Instance.Weight;
                rb.AddExplosionForce(explosionForce*2, rb.transform.position-transform.position, explosionRadius*50, 0.2f, ForceMode.Impulse);
                Destroy(gameObject);
            }
        }
    }

    bool HasNegativeScale(Transform t)
    {
        return t.lossyScale.x < 0 || t.lossyScale.y < 0 || t.lossyScale.z < 0;
    }
        
}
