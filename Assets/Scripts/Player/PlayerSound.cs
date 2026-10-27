using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip extraJumpSound;
    public AudioClip footStepOne;
    public AudioClip footStepTwo;

    public void PlayFootStepOne() => Play(footStepOne);
    public void PlayFootStepTwo() => Play(footStepTwo);
    public void PlayJumpSound() => Play(jumpSound);
    public void PlayExtraJumpSound() => Play(extraJumpSound);
    public void PlayDashSound() => Play(dashSound);

    private void Play(AudioClip clip)
    {
        if (clip && AudioManager.Instance)
        {
            AudioManager.Instance.Play(clip);
        }
    }
}
