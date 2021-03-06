using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MenuManager : MonoBehaviour
{
	public static MenuManager Instance;

	Resolution[] resolutions;
	public Dropdown resolutionDropdown;
	[SerializeField] private AudioMixer _masterMixer;
	[SerializeField] private Slider _volumeSlider;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}

		DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
		resolutions = Screen.resolutions;
		resolutionDropdown.ClearOptions();
		List<string> options = new List<string>();
		int currentResolutionIndex = 0;
		for (int i = 0; i < resolutions.Length; i++)
		{
			string option = resolutions[i].width + "x" + resolutions[i].height;
			options.Add(option);

			if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
			{
				currentResolutionIndex = i;
			}
		}
		resolutionDropdown.AddOptions(options);
		resolutionDropdown.value = currentResolutionIndex;
		resolutionDropdown.RefreshShownValue();

		// Volume Slider
		_volumeSlider.onValueChanged.AddListener(delegate
		{ SetVolumeLevel(); });
	}

	public void SetResolution(int resolutionIndex)
	{
		Resolution resolution = resolutions[resolutionIndex];
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
	}

	public void SetQuality(int qualityIndex)
	{
		QualitySettings.SetQualityLevel(qualityIndex);
	}

	public void SetVolumeLevel()
	{
		// remap the slider value in the mixer range (-80 - 0)
		float mixerVolume = -80 + _volumeSlider.value * 100 * 0.8f;

		_masterMixer.SetFloat("MasterVolume", mixerVolume);
	}

	public void ReturnLauncher()
	{
		PhotonNetwork.LeaveRoom();
		SceneManager.LoadScene("Launcher");
	}

	public void Exit()
	{
		Application.Quit();
	}
}
