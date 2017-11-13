using System;

namespace EcsSync2
{
	public abstract class Message : IReferencable
	{
		protected virtual void Reset()
		{
		}

		#region IReferencable

		IReferenceCounter IReferencable.ReferenceCounter { get; set; }

		protected IReferenceCounter ReferenceCounter => ( (IReferencable)this ).ReferenceCounter;

		void IReferencable.Reset()
		{
			Reset();
		}

		#endregion
	}
}
