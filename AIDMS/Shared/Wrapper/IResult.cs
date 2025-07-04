﻿using System.Collections.Generic;

namespace AIDMS.Shared.Wrapper
{
    public interface IResult
    {
        List<string> Messages { get; set; }
        bool Succeeded { get; set; }
    }

    public interface IResult<out T> : IResult
    {
        T Data { get; }
    }
}
