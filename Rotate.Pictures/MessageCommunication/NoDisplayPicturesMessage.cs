﻿
using System.Collections.Generic;

namespace Rotate.Pictures.MessageCommunication
{
	public class NoDisplayPicturesMessage : IVmCommunication
	{
		public readonly struct NoDisplayParam
		{
			public IEnumerable<int> NoDisplayPics { get; }

			public IDictionary<int, string> NoDisplayDic { get; }

			public NoDisplayParam(IEnumerable<int> noDisplayPics, IDictionary<int, string> noDisplayDic)
			{
				NoDisplayPics = noDisplayPics;
				NoDisplayDic = noDisplayDic;
			}
		}

		public NoDisplayParam Param { get; }

		public NoDisplayPicturesMessage(NoDisplayParam noDisplayParam)
		{
			Param = noDisplayParam;
		}
	}
}
