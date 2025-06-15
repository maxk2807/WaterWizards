using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Client.gamescreen.cards;
using WaterWizard.Shared;

namespace WaterWizard.Server.handler;

public class CardHandler
{
    private readonly GameState? gameState;

    public CardHandler(GameState gameState)
    {
        this.gameState = gameState;
    }

    /// <summary>
    /// Handles the Buying of Cards from a CardStack. Takes a random Card from the corresponding CardStack
    /// of the <see cref="CardType"/> given in <paramref name="reader"/>
    /// </summary>
    /// <param name="peer">The <see cref="NetPeer"/> Client sending the Placement Request</param>
    /// <param name="reader"><see cref="NetPacketReader"/> with the Request Data</param>
    public static void HandleCardBuying(NetManager server, NetPeer peer, NetPacketReader reader)
    {
        string cardType = reader.GetString();
        Console.WriteLine($"[Server] Trying to Buy {cardType} Card");
        if(GameState.UtilityStack == null || GameState.DamageStack == null || GameState.EnvironmentStack == null)
        {
            Console.WriteLine("[Server] Card stacks are not initialized, cannot buy card.");
            return;
        }
        Cards? card = cardType switch
        {
            "Utility" => RandomCard(GameState.UtilityStack), //TODO: Actually Paying
            "Damage" => RandomCard(GameState.DamageStack), //TODO: Actually Paying
            "Environment" => RandomCard(GameState.EnvironmentStack), //TODO: Actually Paying
            _ => throw new Exception(
                "Invalid CardType: "
                    + cardType
                    + " . Has to be a string of either: Utility, Damage or Environment"
            ),
        };
        NetDataWriter writer = new();
        writer.Put("BoughtCard");
        writer.Put(card.Variant.ToString());
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
        if (server == null)
        {
            Console.WriteLine("[Server] GameState.server is null, cannot send card to opponent.");
            return;
        }
        if (server.ConnectedPeerList.Count == 2)
        {
            writer = new();
            writer.Put("OpponentBoughtCard");
            writer.Put(card.Type.ToString());
            var opponent = server.ConnectedPeerList.Find(p => !p.Equals(peer));
            opponent?.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        Console.WriteLine($"[Server] Player_{peer.Port} Bought Card {card.Variant}");
    }

    private static Cards RandomCard(List<Cards> stack)
    {
        var index = (int)(stack.Count * Random.Shared.NextSingle());
        return stack[index];
    }

    /// <summary>
    /// Handles the Casting of Cards from the <see cref="NetPeer"/>s hand. //TODO: handle Mana Cost.
    /// Calls the Ability in <see cref="CardAbilities"/>
    /// </summary>
    /// <param name="peer">The <see cref="NetPeer"/> Client sending the Placement Request</param>
    /// <param name="reader"><see cref="NetPacketReader"/> with the Request Data</param>
    public void HandleCardCasting(NetManager server, NetPeer peer, NetPacketReader reader, GameState gameState)
    {
        //TODO: Handle Mana Cost
        string cardVariantString = reader.GetString();
        int cardX = reader.GetInt();
        int cardY = reader.GetInt();
        if (Enum.TryParse<CardVariant>(cardVariantString, out var variant))
        {
            // Gegner finden
            var defender = server.ConnectedPeerList.Find(p => !p.Equals(peer));
            if (gameState == null)
            {
                Console.WriteLine("[Server] GameState is null, cannot handle CardCast.");
                return;
            }
            if (defender != null)
            {
                CardAbilities.HandleAbility(variant, gameState, new Vector2(cardX, cardY), peer, defender);
            }
            else
            {
                Console.WriteLine("[Server] Kein Gegner gefunden für CardCast.");
            }
        }
        else
        {
            Console.WriteLine($"[Server] Casting Failed. Variant {cardVariantString} unknown");
        }
    }

    internal void CardActivation(CardVariant variant, int duration)
    {
        if (gameState == null)
        {
            Console.WriteLine("[Server] GameState is null, cannot activate card.");
            return;
        }
        Console.WriteLine($"[Server] Activate Card {variant} for {duration} seconds");
        GameState.ActiveCards?.Add(new Cards(variant) { remainingDuration = duration * 1000f });

        // Sofort die aktive Karte an alle Clients senden
        foreach (var player in gameState.players)
        {
            SendActiveCardsUpdate(player);
        }
    }

    public void UpdateActiveCards(float passedTime)
    {
        if (GameState.ActiveCards == null || GameState.ActiveCards.Count == 0)
        {
            return; 
        }
        if (gameState == null)
        {
            Console.WriteLine("[Server] GameState is null, cannot update active cards.");
            return;
        }
        for (int i = GameState.ActiveCards.Count - 1; i >= 0; i--)
        {
            var card = GameState.ActiveCards[i];
            card.remainingDuration -= passedTime;

            if (card.remainingDuration <= 0)
            {
                // Karte ist abgelaufen
                Console.WriteLine($"[Server] Card {card.Variant} expired, removing from active cards");
                GameState.ActiveCards.RemoveAt(i);

                foreach (var player in gameState.players)
                {
                    SendActiveCardsUpdate(player);
                }

                if (card.Variant == CardVariant.Thunder)
                {
                    Console.WriteLine("\n[Server] Thunder Card expired");
                    Console.WriteLine("----------------------------------------");
                    foreach (var player in gameState.players)
                    {
                        NetDataWriter resetWriter = new();
                        resetWriter.Put("ThunderReset");
                        player.Send(resetWriter, DeliveryMethod.ReliableOrdered);
                        Console.WriteLine($"Sent ThunderReset to player: {player}");
                    }
                    Console.WriteLine("----------------------------------------\n");
                }
                continue;
            }

            CardAbilities.HandleActivationEffect(card, passedTime);

            if (card.Variant == CardVariant.Thunder)
            {
                GameState.thunderTimer -= passedTime / 1000f;

                if (GameState.thunderTimer <= 0)
                {
                    GameState.thunderTimer = GameState.THUNDER_INTERVAL;
                    Console.WriteLine("\n[Server] Thunder Strike Round");
                    Console.WriteLine("----------------------------------------");

                    for (int boardIndex = 0; boardIndex < 2; boardIndex++)
                    {
                        var targetPlayer = gameState.players[boardIndex];
                        var attacker = gameState.players[boardIndex == 0 ? 1 : 0];
                        
                        Console.WriteLine($"Generating 2 thunder strikes for Board[{boardIndex}] (Player: {targetPlayer})");
                        
                        for (int strikeNum = 0; strikeNum < 3; strikeNum++)
                        {
                            int x = Random.Shared.Next(0, GameState.boardWidth);
                            int y = Random.Shared.Next(0, GameState.boardHeight);

                            bool hit = HandleThunderStrike(attacker, targetPlayer, x, y);
                            
                            SendThunderVisualEffect(gameState.players, boardIndex, x, y, hit);
                        }
                    }
                    Console.WriteLine("----------------------------------------\n");
                }
            }
        }
    }
    
    private static void SendActiveCardsUpdate(NetPeer player)
    {
        var writer = new NetDataWriter();
        writer.Put("ActiveCards");
        if(GameState.ActiveCards == null)
        {
            Console.WriteLine("[Server] GameState.ActiveCards is null, cannot send active cards.");
            return;
        }
        writer.Put(GameState.ActiveCards.Count);
        foreach (var card in GameState.ActiveCards)
        {
            writer.Put(card.Variant.ToString());
            writer.Put(card.remainingDuration);
        }
        player.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    /// <summary>
    /// Handles a single thunder strike and returns if it was a hit
    /// </summary>
    private bool HandleThunderStrike(NetPeer attacker, NetPeer targetPlayer, int x, int y)
    {
        var ships = ShipHandler.GetShips(targetPlayer);
        bool hit = false;

        foreach (var ship in ships)
        {
            if (x >= ship.X && x < ship.X + ship.Width &&
                y >= ship.Y && y < ship.Y + ship.Height)
            {
                hit = true;
                bool newDamage = ship.DamageCell(x, y);
                
                Console.WriteLine($"    Thunder hit ship at ({ship.X}, {ship.Y}), new damage: {newDamage}");

                if (newDamage)
                {
                    if (ship.IsDestroyed)
                    {
                        Console.WriteLine($"    Thunder destroyed ship at ({ship.X}, {ship.Y})!");
                        ShipHandler.SendShipReveal(attacker, ship, gameState!);
                    }
                    else
                    {
                        CellHandler.SendCellReveal(attacker, targetPlayer, x, y, true);
                    }
                }
                else
                {
                    CellHandler.SendCellReveal(attacker, targetPlayer, x, y, true);
                }
                break;
            }
        }

        if (!hit)
        {
            Console.WriteLine($"    Thunder missed at ({x}, {y})");
            CellHandler.SendCellReveal(attacker, targetPlayer, x, y, false);
        }

        return hit;
    }

    /// <summary>
    /// Sends thunder visual effects to all clients
    /// </summary>
    /// <param name="boardIndex">The referenced board</param>
    /// <param name="hit">Boolean if a ship was hit or not</param>
    /// <param name="players">The connected player</param>
    /// <param name="x">x-Coordinate</param>
    /// <param name="y">y-Coordinate</param>
    private void SendThunderVisualEffect(NetPeer[] players, int boardIndex, int x, int y, bool hit)
    {
        foreach (var client in players)
        {
            NetDataWriter thunderWriter = new();
            thunderWriter.Put("ThunderStrike");
            thunderWriter.Put(boardIndex);
            thunderWriter.Put(x);
            thunderWriter.Put(y);
            thunderWriter.Put(hit);
            
            client.Send(thunderWriter, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"    Sent ThunderStrike visual to {client} for Board[{boardIndex}] at ({x},{y}) hit={hit}");
        }
    }
}