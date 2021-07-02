using System;

namespace Rotate.Pictures.EventAggregator
{
	public class SliderPositionEventArgs : EventArgs
	{
		public double Minimum { get; }
		
		public double Maximum { get; }

		public double Value { get; }

		public SliderPositionEventArgs(double min, double max, double val)
		{
			Minimum = min;
			Maximum = max;
			Value = val;
		}
	}
}
