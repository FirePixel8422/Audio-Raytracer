using System;


namespace Fire_Pixel.Utility
{
    /// <summary>
    /// Container that stores an <see cref="Action"/> which can be subscribed to and is invoked only once.
    /// If subscribed to after invoke already happened, call the subscriber instantly
    /// </summary>
    public class CompletionAction
    {
        private event Action InternalAction;
        private bool hasExecuted;
        public bool HasExecuted => hasExecuted;


        public void Invoke()
        {
            InternalAction?.Invoke();
            InternalAction = null;
            hasExecuted = true;
        }
        public static CompletionAction operator +(CompletionAction e, Action action)
        {
            if (!e.hasExecuted)
            {
                e.InternalAction += action;
            }
            else
            {
                action?.Invoke();
            }
            return e;
        }
        public static CompletionAction operator -(CompletionAction e, Action action)
        {
            if (!e.hasExecuted)
            {
                e.InternalAction -= action;
            }
            return e;
        }
    }

    /// <summary>
    /// Container that stores an <see cref="Action{T}"/> which can be subscribed to and is invoked only once. If subscribed to after invoke already happened, call the subscriber instantly
    /// </summary>
    public class CompletionAction<T>
    {
        private event Action<T> InternalAction;
        private bool hasExecuted;
        private T invokedValue;
        public bool HasExecuted => hasExecuted;


        public void Invoke(T value)
        {
            if (hasExecuted) return;

            InternalAction?.Invoke(value);
            InternalAction = null;
            hasExecuted = true;
            invokedValue = value;
        }
        public static CompletionAction<T> operator +(CompletionAction<T> e, Action<T> action)
        {
            if (!e.hasExecuted)
            {
                e.InternalAction += action;
            }
            else
            {
                action?.Invoke(e.invokedValue);
            }
            return e;
        }
        public static CompletionAction<T> operator -(CompletionAction<T> e, Action<T> action)
        {
            if (!e.hasExecuted)
            {
                e.InternalAction -= action;
            }
            return e;
        }
    }
}