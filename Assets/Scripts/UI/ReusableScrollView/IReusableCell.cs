namespace UI.ReusableScrollView
{
    public interface IReusableCell<T>
    {
        void Setup(T data, int index);
    }
}