using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bouyant : MonoBehaviour
{
    bool inWater;
    Mesh mesh;
    float bouyantForce;
    float waterDensity;
    Rigidbody rb;
    float initDrag;
    public ParticleSystem particle;
    void Start()
    {
        waterDensity = 10f;
        rb = transform.GetComponent<Rigidbody>();
        rb.drag = 0.5f + rb.mass/5f;
        initDrag = rb.drag;
        rb.angularDrag = 1;
        mesh = GetComponent<MeshFilter>().sharedMesh;
    }

    void FixedUpdate()
    {
        rb.AddForce(new Vector3(0, -9f * rb.mass, 0), ForceMode.Acceleration);
        float density = rb.mass/(transform.localScale.x * transform.localScale.y * transform.localScale.z);
        if (density <= waterDensity)
        {
            UpdateBouyancy();
        }
    }

    void UpdateBouyancy()
    {
        float distanceToSurface = 0;
        RaycastHit hitDistance;
        Ray rayDistance = new Ray(transform.position - new Vector3(0, transform.localScale.y / 2, 0), Vector3.up);
        Debug.DrawRay(transform.position - new Vector3(0, 0.5f, 0), Vector3.up, Color.red);

        var hits = Physics.RaycastAll(rayDistance, 100f); // get all hits
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.isTrigger && hit.collider.gameObject.name == "WaterCollider")
            {
                distanceToSurface = hit.distance;
                break;
            }
        }

        if (distanceToSurface > 0)
        {
            particle.transform.position = transform.position + new Vector3(0, distanceToSurface, 0);

            if (particle != null && !inWater && distanceToSurface > 0.2f)
            {
                particle.Play();
                inWater = true;
            }
            rb.drag = initDrag * 2;
            float percentSubmerged = distanceToSurface / transform.localScale.y;
            percentSubmerged = Mathf.Clamp(percentSubmerged, 0, 1);
            float displaced = waterDensity * (transform.localScale.x * transform.localScale.y * transform.localScale.z) * percentSubmerged;
            rb.AddForceAtPosition(new Vector3(0, waterDensity * 9f * displaced, 0), transform.position, ForceMode.Force);
        }
        else
        {
            inWater = false;
            rb.drag = initDrag;
        }

    }
}
