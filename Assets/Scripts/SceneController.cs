using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
	public void GoToMenu()
	{
		SceneManager.LoadScene("Menu");
	}

	public void StartGame()
	{
		SceneManager.LoadScene("GameScene");
	}
}
