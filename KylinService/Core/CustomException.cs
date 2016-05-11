using System;

namespace KylinService.Core
{
    /// <summary>
    /// 自定义异常
    /// </summary>
    public class CustomException : Exception
    {
        public CustomException(string message) : base(message) { }
    }
}
