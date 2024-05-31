using UnityEngine;

public class LinksToAnimators : MonoBehaviour
{
    public void StartPlayerAnim()
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().Dead(1, 1);
    }
    public void StartWindowAnim()
    {
        GameObject.FindGameObjectWithTag("PlayerWindow").GetComponent<WindowControl>().ChangeWindowsStage();
    }


}
