using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SignageInteraction : AttractorLogic
{


    override public void Reset()
    {
        base.Reset();
        updateActiveStatus(true);

    }
    private void updateActiveStatus(bool status)
    {
        var newMaterial = status ? activeMaterial : inactiveMaterial;
        var r = gameObject.GetComponent<Renderer>();
        if (r != null) { r.material = newMaterial; }

        var bc = gameObject.GetComponent<Collider>();
        if (bc != null) { bc.enabled = status; }

    }



    public Material activeMaterial;
    public Material inactiveMaterial;


    override public bool OnAgentEncountered()
    {
        base.OnAgentEncountered();
        updateActiveStatus(false);
        // Debug.Log("Sign encountered");
        if (myArea == null)
        {
            return true;
        }

        var entrances = myArea.gameObject.GetComponentsInChildren<EntranceInteraction>();
        if (entrances.Where(m => m.IsActivated).Count() > 0)
        {
            // target already defined in space
            return true;
        }
        var doorOptions = entrances.Count();
        if (doorOptions < 1)
        {
            // no doors available
            return true;
        }

        int rInt = Random.Range(0, doorOptions);
        var door = entrances[rInt];
        door.activateTarget(activeMaterial);



        return true;

    }
}
