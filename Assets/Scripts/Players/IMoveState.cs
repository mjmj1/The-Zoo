namespace Players
{
    public interface IMoveState
    {
        bool CanMove { get; set; }
        bool IsSpinning { get; set; }
    }
}