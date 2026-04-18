using UnityEngine;

public class HelpPointLogic : MonoBehaviour
{
    public Friendly_Robot friendlyRobot;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        Debug.Log("player");
        friendlyRobot.SetPlayerOnArm(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            friendlyRobot.SetPlayerOnArm(false);
        }
    }
}