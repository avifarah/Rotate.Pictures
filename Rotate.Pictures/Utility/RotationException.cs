using System;

namespace Rotate.Pictures.Utility
{
    public class RotationException : Exception
    {
        private string _currPicture = null;

        public RotationException(string message) : base(message) { }

        public RotationException(string message, Exception innerException) : base(message, innerException) { }

        public RotationException(string currentPicture, string message, Exception innException = null)
            : base(message, innException)
        {
            _currPicture = currentPicture;
        }
    }
}
