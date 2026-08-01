namespace EricksonLopez.Result;

/// <summary>
/// Internal state discriminator for Result struct memory safety.
/// </summary>
internal enum ResultState : byte
{
    Uninitialized = 0,
    Success = 1,
    Failure = 2
}
