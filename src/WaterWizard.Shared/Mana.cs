namespace WaterWizard.Shared
{
    public class Mana
    {
        /// <summary>
        /// The current amount of mana.
        /// </summary>
        public int CurrentMana { get; private set; }

        public Mana(int initialAmount = 0)
        {
            CurrentMana = initialAmount;
        }

        public void Add(int amount)
        {
            CurrentMana += amount;
        }

        public bool Spend(int cost)
        {
            if (CurrentMana >= cost)
            {
                CurrentMana -= cost;
                return true;
            }
            return false;
        }
    }
}
