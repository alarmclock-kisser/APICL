﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APICL.Shared
{
	public static class CommonStatics
	{
		public static int MaxAvailableWorkersCount { get; } = Environment.ProcessorCount;

		public static int AdjustWorkersCount(int maxWorkers = 0)
		{
			int totalCores = Environment.ProcessorCount;

			// Sonderfall: 0 bedeutet volle CPU-Auslastung
			if (maxWorkers == 0)
			{
				return totalCores;
			}

			// Negative Werte: CPU-Kerne freihalten (z.B. -2 = 2 Kerne weniger als Maximum)
			if (maxWorkers < 0)
			{
				int adjusted = totalCores + maxWorkers; // maxWorkers ist negativ, also Subtraktion
				return Math.Max(1, adjusted); // Mindestens 1 Worker
			}

			// Positive Werte: Direkte Vorgabe, aber nicht mehr als verfügbare Kerne
			return Math.Clamp(maxWorkers, 1, totalCores);
		}
	}
}
