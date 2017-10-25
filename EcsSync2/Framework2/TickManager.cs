using System;
using System.Collections.Generic;
using System.Text;

namespace EcsSync2.Framework2
{
	class TickableScheduler
	{
		public enum TickMode
		{
			Sync,
			Reconcilation,
			Prediction,
			Interpolation,
		}

		public interface ITickContext
		{
			TickMode Mode { get; }

			uint Time { get; }
		}

		public ITickContext CurrentContext { get; protected set; }
	}
}
