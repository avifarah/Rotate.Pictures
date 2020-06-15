using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Rotate.Pictures.Utility
{
	/// <summary>
	/// Purpose:
	///		Communication conduit for MetaData
	/// </summary>
	public class PictureMetaDataTransmission : IEqualityComparer<PictureMetaDataTransmission>, IEquatable<PictureMetaDataTransmission>
	{
		public string PictureFolder { get; set; }

		public string FirstPictureToDisplay { get; set; }

		public string StillPictureExtensions { get; set; }

		public string MotionPictureExtensions { get; set; }

		#region IEqualityComparer

		public bool Equals(PictureMetaDataTransmission lhs, PictureMetaDataTransmission rhs)
		{
			if (ReferenceEquals(lhs, null)) return false;
			if (ReferenceEquals(rhs, null)) return false;
			if (ReferenceEquals(lhs, rhs)) return true;
			if (lhs.PictureFolder != rhs.PictureFolder) return false;
			if (lhs.FirstPictureToDisplay != rhs.FirstPictureToDisplay) return false;
			if (lhs.StillPictureExtensions != rhs.StillPictureExtensions) return false;
			if (lhs.MotionPictureExtensions != rhs.MotionPictureExtensions) return false;
			return true;
		}

		public int GetHashCode(PictureMetaDataTransmission metaData) => metaData.GetHashCode();

		#endregion

		#region IEquatable

		public override bool Equals(object other)
		{
			if (ReferenceEquals(this, other)) return true;
			if (ReferenceEquals(other, null)) return false;
			if (other is PictureMetaDataTransmission metaData) return Equals(this, metaData);
			return false;
		}

		public override int GetHashCode() => $"{PictureFolder}||{FirstPictureToDisplay}||{StillPictureExtensions}||{MotionPictureExtensions}".GetHashCode();

		#endregion

		public bool Equals(PictureMetaDataTransmission other) => Equals(this, other);

		public static bool operator ==(PictureMetaDataTransmission lhs, PictureMetaDataTransmission rhs) => lhs?.Equals(lhs, rhs) ?? false;

		public static bool operator !=(PictureMetaDataTransmission lhs, PictureMetaDataTransmission rhs) => !(lhs == rhs);
	}
}
