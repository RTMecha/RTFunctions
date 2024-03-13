﻿using System;
using UnityEngine;

namespace RTFunctions.Functions
{
	/// <summary>
	/// Class for performing specific actions on components.
	/// </summary>
	public class ComponentAction : Exists
	{
		public Component Component { get; set; }
		public Type Type { get; set; }
		public Action<Component> Action { get; set; }
	}
}
