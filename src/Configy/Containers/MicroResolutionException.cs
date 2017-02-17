using System;

namespace Configy.Containers
{
	[Serializable]
	public class MicroResolutionException : Exception
	{
		public MicroResolutionException(string message) : base(message)
		{
		}
	}
}
