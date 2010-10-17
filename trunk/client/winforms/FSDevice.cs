using System;
using System.Collections.Generic;
using System.Text;

namespace FishnSpots
{
	public interface FSDevice
	{
		string Name
		{
			get;
			set;
		}

		void SetParameterValue(string ParameterName, object ParameterValue);
		object GetParameterValue(string ParameterName);

		void EnableDevice();
		void DisableDevice();
	}
}
