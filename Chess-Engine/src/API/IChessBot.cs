
namespace ChessEngine.API
{
    public interface IChessBot
    {
        Move Think(Board board, Timer timer);
    }
}
