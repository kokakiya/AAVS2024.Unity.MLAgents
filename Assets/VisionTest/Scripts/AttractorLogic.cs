using System.Collections;
using UnityEngine;

public class AttractorLogic : MonoBehaviour
{
    public bool respawn;

    public TrainArea myArea;
    [HideInInspector]
    public bool isStatic = true;
    public float yPosition = 0.49f;
    public float disableDurationSeconds = 1;
    public bool restartsEpisode = false;

    public float myReward = 0f;

    virtual public void Reset()
    {
        gameObject.SetActive(true);
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (!isStatic)
        {
            transform.position = new Vector3(Random.Range(-myArea.range, myArea.range),
          yPosition,
          Random.Range(-myArea.range, myArea.range)) + myArea.transform.position;
        }
    }

    private void Respawn()
    {
        gameObject.SetActive(false);
        Invoke("Reset", disableDurationSeconds);



    }

    private IEnumerator RespawnCo()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(disableDurationSeconds);
        Reset();

    }

    virtual public bool OnAgentEncountered()
    {

        // Debug.Log($"OnAgentEncountered {Time.frameCount}");

        if (respawn || isStatic)
        {
            if (respawn || disableDurationSeconds > 0)
            {

                Respawn();
            }
        }
        else if (!isStatic)
        {
            Destroy(gameObject);
        }
        return true;
    }
}
