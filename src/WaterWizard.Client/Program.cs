using Raylib_cs;

namespace WaterWizard.Client
{
    static class Program
    {
        static void Main()
        {
            const int screenWidth = 800;
            const int screenHeight = 600;

            try
            {
                Raylib.InitWindow(screenWidth, screenHeight, "WaterWizards - Battleship Game");

                Raylib.SetTargetFPS(60);

                while (!Raylib.WindowShouldClose())
                {
                    Raylib.BeginDrawing();

                    Raylib.ClearBackground(Color.Beige);

                    Raylib.DrawText("Welcome to WaterWizards!", screenWidth/3, screenHeight/2, 20, Color.DarkBlue);

                    Raylib.EndDrawing();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                if (Raylib.IsWindowReady())
                {
                    Raylib.CloseWindow();
                }
            }
        }
    }
}