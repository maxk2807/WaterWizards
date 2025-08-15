using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.environment;

public class RiseSunCard : IEnvironmentCard
{
    public CardVariant Variant => CardVariant.RiseSun;

    public Vector2 AreaOfEffect => new(); // affects entire battlefield

    public bool HasSpecialTargeting => false;

    public bool ExecuteEnvironment(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        Console.WriteLine("\n[Server] RiseSunCard activated – resetting all environment effects");
        Console.WriteLine("----------------------------------------");

        EnvironmentCardFactory.ResetEnvironmentEffects(gameState);

        // Inform all clients to remove environment visuals if necessary
        foreach (var player in gameState.players)
        {
            NetDataWriter writer = new();
            writer.Put("ResetEnvironment");
            player.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"Sent ResetEnvironment to player: {player}");
        }

        Console.WriteLine("----------------------------------------\n");
        return true;
    }

    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer defender)
        => true;
}