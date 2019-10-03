namespace WB.Enumerator.Native.WebInterview
{
    public static class InScopeExecutor
    {
        public static IInScopeExecutor Current { get; private set; }

        public static void Init(IInScopeExecutor executor)
        {
            Current = executor;
        }
    }
}
