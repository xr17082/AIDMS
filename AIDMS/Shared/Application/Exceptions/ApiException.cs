﻿using System;
using System.Globalization;

namespace AIDMS.Shared.Application.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException() : base()
        {
        }

        public ApiException(string message) : base(message)
        {
        }

        public ApiException(string message, params object[] args) : base(string.Format(CultureInfo.InvariantCulture, message, args))
        {
        }
    }
}
