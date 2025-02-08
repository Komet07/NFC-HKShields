
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace StarWarsShields
{
	public class RateLimitedLogger
	{
		private Dictionary<int, float> _limiter = new Dictionary<int, float>();
		private float _threshold;

		public RateLimitedLogger(float _threshold)
		{
			this._threshold = _threshold;
		}

		public void LogLimited(string message)
		{
			var id = message.GetHashCode();
			if (!_limiter.ContainsKey(id) || Time.time > _limiter[id] + _threshold)
			{
				Debug.Log(message);
				_limiter.AddWithOverwrite(id, Time.time);
			}
		}
	}
}
	
