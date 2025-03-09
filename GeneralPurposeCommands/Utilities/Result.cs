using System;
using System.Collections.Generic;
using System.Text;

namespace GeneralPurposeCommands.Utilities
{
    public class Result<T>
    {
        public bool IsOk { get; set; }
        public T Value { get; set; }
        public string ErrorMessage { get; set; }

        public Result(T value)
        {
            IsOk = true;
            Value = value;
        }

        public Result(string errorMessage)
        {
            IsOk = false;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Ok(T value) => new(value);

        public static Result<T> Err(string errorMessage) => new(errorMessage);

        public void Match(Action<Result<T>> Ok, Action<Result<T>> Err)
        {
            if (IsOk)
            {
                Ok(this);
            }
            else
            {
                Err(this);
            }
        }
    }
}
