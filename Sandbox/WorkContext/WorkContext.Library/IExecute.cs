using System;
using System.Data.Common;
using Smart.ComponentModel;

namespace WorkContext.Library
{
    public interface IExecute
    {
        int Execute(NotificationValue<int> value);
    }
}
