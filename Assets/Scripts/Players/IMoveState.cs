using System;

namespace Players
{
    public interface IMoveState
    {
        bool CanMove { get; set; }
        bool IsJumping { get; set; }
    }
}