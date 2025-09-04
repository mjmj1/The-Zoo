namespace Unit
{
    public interface IActionState
    {
        bool CanMove { get; set; }
        bool IsJumping { get; set; }
    }
}