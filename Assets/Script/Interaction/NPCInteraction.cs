using System;
using UnityEngine;

public class NPCInteraction : InteractionController
{
	private string npcName = "Guard";
	private float lastInteractionTime;

	protected override void OnInteract()
	{
		ShowDialogue();
	}

	private void ShowDialogue()
	{
		lastInteractionTime = Time.time;
		Debug.Log($"{npcName}: Hello, traveler! Welcome to our town.");
	}
}
