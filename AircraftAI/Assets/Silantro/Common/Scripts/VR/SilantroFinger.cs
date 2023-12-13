using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sends the message required to interact with VR components
/// </summary>


public class SilantroFinger : MonoBehaviour
{
	// ------------------------------------------------------------------------------------------- 
	private void OnTriggerEnter(Collider other)
	{
		if (other.transform != null)
		{
			//IDENTIFY INSTRUMENT
			SilantroButton clickedButton = other.gameObject.GetComponent<SilantroButton>();

			if (clickedButton != null)
			{
				if (clickedButton.buttonAction == SilantroButton.ButtonAction.Press)
				{
					if (clickedButton != null)
					{
						clickedButton.Press();
						clickedButton.ToggleButton();
					}
				}

				if (clickedButton.buttonAction == SilantroButton.ButtonAction.Flip)
				{
					if (clickedButton != null)
					{
						clickedButton.Flip();
						clickedButton.ToggleKnob();
					}
				}
			}
		}
	}
}
