namespace WorkBuild.Library
{
    using Smart.ComponentModel;

    public class Engine
    {
        public T Execute<T>(object arg)
        {
            if (arg is IValueHolder<T> holder)
            {
                return holder.Value;
            }

            return default;
        }
    }
}
