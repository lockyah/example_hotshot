using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager LM;

    public int CheckpointIndex = -1;
    public Vector3 CheckpointPosition;
    public bool CheckpointFacingRight = true;

    private CinemachineTargetGroup CameraTarget;

    void OnEnable()
    {
        if(LM == null)
        {
            DontDestroyOnLoad(gameObject);
            LM = this;

            CameraTarget = transform.GetChild(0).GetComponent<CinemachineTargetGroup>();
        } else
        {
            Destroy(gameObject);
        }
    }

    public void LoadLevel(int index)
    {
        CheckpointIndex = -1; //Player is positioned in start point automatically, should later check for an entry position
        CheckpointPosition = Vector3.zero;
        SceneManager.LoadScene(index);
    }

    public void ResetCameraOnPlayer()
    {
        //Empty out the target list
        foreach(CinemachineTargetGroup.Target t in CameraTarget.m_Targets)
        {
            CameraTarget.RemoveMember(t.target);
        }

        GameObject player = CutsceneDirector.CD.PlayerRef.gameObject;
        Camera.main.transform.position = player.transform.position;

        CameraTarget.AddMember(player.transform.GetChild(1), 1.5f, 1);
        CameraTarget.AddMember(player.transform.GetChild(2), 0.25f, 1);

    }

    public void ReturnToCheckpoint()
    {
        CanvasController.CC.SetCurrentHealth(CanvasController.CC.GetMaxHealth());
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
