using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
	public GameObject options;

	public GameObject menu;

    public GameObject credit;

    public void Quit()
	{
		Application.Quit();
	}

	public void Play()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

	}

	public void GoToOptions()
	{
		menu.SetActive(false);
		options.SetActive(true);
	}
	public void GoToMenu()
	{
		menu.SetActive(true);
        credit.SetActive(false);
		options.SetActive(false);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			SceneManager.LoadScene(0);
		}
	}

    public void OnCreditsClicked()
    {
        menu.SetActive(false);
        credit.SetActive(true);
    }
}
