using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Mono.Cecil;

namespace Versioning.Issues
{
	public class MissingEventIssue : ICompatibilityIssue
	{
		public EventDefinition MissingEvent { get; }
		public MissingEventIssue(EventDefinition @event)
		{
			if (@event == null) throw new ArgumentNullException(nameof(@event));

			this.MissingEvent = @event;
		}
	}
}
