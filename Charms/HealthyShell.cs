using Modding;

namespace Fyrenest
{
    internal class HealthyShell : Charm
    {
        public static readonly HealthyShell instance = new();
        public override bool Placeable => false;
        public override string Sprite => "HealthyShell.png";
        public override string Name => "Healthy Shell";
        public override string Description => "Makes your shell glossy and black.\n\nYou continously gain health when hurt.";
        public override int DefaultCost => 4;
        public override string Scene => "Town";
        public override float X => 0f;
        public override float Y => 0f;
        private HealthyShell() {}

        public override CharmSettings Settings(SaveSettings s) => s.HealthyShell;

        public override void Hook()
        {
            ModHooks.HeroUpdateHook += Update;
        }

        private const int PhysicsFramesPerSecond = 50;

        private const int ChargeInterval = 30 * PhysicsFramesPerSecond;

        private static int ChargeTimer = 0;

        private void Update()
        {
            if (Equipped())
            {
                ChargeTimer++;
                if (ChargeTimer == ChargeInterval)
                {
                    Modding.Logger.Log("Timer for HealthyShell Up.");
                    ChargeTimer = 0;
                    if (PlayerData.instance.health == PlayerData.instance.maxHealth)
                    {
                        Modding.Logger.Log("Player has max health. Giving player soul.");
                        HeroController.instance.AddMPCharge(11);
                    }
                    else {
                        Modding.Logger.Log("Giving player health.");
                        HeroController.instance.AddHealth(1);
                        HeroController.instance.TakeMP(11);
                    }
                }
            }
            return;
        }
    }
}