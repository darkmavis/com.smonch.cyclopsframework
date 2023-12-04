namespace Smonch.CyclopsFramework
{
    public struct CyclopsStateUpdateContext
    {
        public CyclopsGame.UpdateSystem UpdateSystem { get; internal set; }
        public bool IsLayered { get; internal set; }
    }
}