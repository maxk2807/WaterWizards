using Microsoft.VisualBasic;
using WaterWizard.Shared;

namespace WaterWizard.Server;

public class GameState
{
    private static readonly int boardWidth = 12;
    private static readonly int boardHeight = 10;
    private Cell[,] Player1 = new Cell[boardWidth, boardHeight];
    private Cell[,] Player2 = new Cell[boardWidth, boardHeight];

    List<Cards> player1Hand = [];
    List<Cards> player2Hand = [];

    List<Cards> activeCards = [];

    List<Cards> utilityStack;
    List<Cards> damageStack;
    List<Cards> environmentStack;

    List<Cards> graveyard;

    private GameState(Cell[,] player1Board, Cell[,] player2Board, List<Cards> player1Hand, List<Cards> player2Hand,
     List<Cards> activeCards, List<Cards> utilityStack, List<Cards> damageStack, List<Cards> environmentStack, List<Cards> graveyard)
    {
        Player1 = player1Board;
        Player2 = player2Board;
        this.player1Hand = player1Hand;
        this.player2Hand = player2Hand;
        this.activeCards = activeCards;
        this.utilityStack = utilityStack;
        this.damageStack = damageStack;
        this.environmentStack = environmentStack;
        this.graveyard = graveyard;
    }

    public static GameState StartNewGame()
    {
        Cell[,] player1Board = new Cell[boardWidth, boardHeight];
        Cell[,] player2Board = new Cell[boardWidth, boardHeight];
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                player1Board[i,j] = new(CellState.Empty);
                player2Board[i,j] = new(CellState.Empty);
            }
        }
        List<Cards> player1Hand = [];
        List<Cards> player2Hand = [];
        List<Cards> activeCards = [];

        List<Cards> utilityStack = Cards.GetCardsOfType(CardType.Utility);
        utilityStack.AddRange(Cards.GetCardsOfType(CardType.Healing));
        List<Cards> damageStack = Cards.GetCardsOfType(CardType.Damage);
        List<Cards> environmentStack = Cards.GetCardsOfType(CardType.Environment);

        List<Cards> graveyard = [];

        return new(player1Board, player2Board, player1Hand, player2Hand,activeCards, utilityStack, damageStack, environmentStack, graveyard);
    }
}