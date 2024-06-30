using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntranceInteraction : AttractorLogic
{

    [HideInInspector]
    public bool IsActivated = false;
    public Material originalMaterial;

    // Start is called before the first frame update
    override public void Reset()
    {
        base.Reset();
        activateTarget();

    }


    public void activateTarget(Material activeMaterial = null)
    {
        IsActivated = activeMaterial != null;
        var newMaterial = IsActivated ? activeMaterial : originalMaterial;
        var r = gameObject.GetComponent<Renderer>();
        if (r != null) { r.material = newMaterial; }

        /* var bc = gameObject.GetComponent<Collider>();
        if (bc != null) { bc.enabled = IsActivated; } */

        //  Debug.Log($"Activate entrance {IsActivated}");

    }

    override public bool OnAgentEncountered()
    {
        if (!IsActivated)
        {
            return false;
        }
        return base.OnAgentEncountered();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
