using System;

namespace EcsSync2
{
	public abstract class Message : IReferencable
	{
		protected virtual void Reset()
		{
			throw new NotImplementedException();
		}

		#region IReferencable

		IReferenceCounter IReferencable.ReferenceCounter { get; set; }

		void IReferencable.Reset()
		{
			Reset();
		}

		#endregion
	}
}
