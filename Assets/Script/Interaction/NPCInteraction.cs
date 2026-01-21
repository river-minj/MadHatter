using System;
using UnityEngine;

public class NPCInteraction : InteractionController
{
	private string npcName = "Guard";
	private float lastInteractionTime;

	[SerializeField]
	string[] _npcMessage
		= {

		"Hello there, traveler! Welcome to our village.",
		"The weather is quite nice today, isn't it?",
		"Be careful if you venture into the forest.",
		"There are rumors of bandits lurking about.",
		};

	protected override void OnInteract()
	{
		ShowDialogue();
	}

	private void ShowDialogue()
	{
		lastInteractionTime = Time.time;
		GameManager.Instance?.StartDialogue(_npcMessage);
	}
}
