using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class with some custom extensions.
/// </summary>
static class MyExtensions
{
	private static System.Random rng = new System.Random();  

	/// <summary>
	/// Shuffle the specified list.
	/// </summary>
	/// <param name="list">The list.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static void Shuffle<T>(this IList<T> list)  
	{  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next(n + 1);  
			T value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
	}

	/// <summary>
	/// Sets the layer with support for including all children.
	/// </summary>
	/// <param name="parent">The game object itself.</param>
	/// <param name="layer">The new layer.</param>
	/// <param name="includeChildren">If set to <c>true</c> include children.</param>
	public static void SetLayer(this GameObject parent, int layer, bool includeChildren = true)
	{
		parent.layer = layer;
		if (includeChildren)
		{
			foreach (Transform trans in parent.transform.GetComponentsInChildren<Transform>(true))
			{
				trans.gameObject.layer = layer;
			}
		}
	}
}
