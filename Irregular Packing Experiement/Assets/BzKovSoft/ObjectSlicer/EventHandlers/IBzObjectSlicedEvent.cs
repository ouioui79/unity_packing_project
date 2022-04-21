using UnityEngine;

namespace BzKovSoft.ObjectSlicer.EventHandlers
{
	public interface IBzObjectSlicedEvent
	{
		void ObjectSliced(GameObject original, GameObject resutlNeg, GameObject resultPos);
	}
}
