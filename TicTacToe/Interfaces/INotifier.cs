namespace TicTacToe.Interfaces
{
    public interface INotifier<in T>
    {
        void AddListener(T listener);

        void RemoveListener(T listener);
    }
}
