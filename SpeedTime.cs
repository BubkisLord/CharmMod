using Modding;
using UnityEngine;
using GlobalEnums;

namespace CharmMod
{
    internal class SpeedTime : Charm
    {
        public static readonly SpeedTime Instance = new();
        public override string Sprite => "SpeedTime.png";
        public override string Name => "Speed Time";
        public override string Description => "When holding this charm, everything seems to speed up slightly.\n\nThis charm alters the very fabric of time. When worn, the bearer and anything around it speeds up.";
        public override int DefaultCost => 2;
        public override string Scene => "Ruins2_11";
        public override float X => 0f;
        public override float Y => 0f;

        private SpeedTime() {}

        public override CharmSettings Settings(SaveSettings s) => s.SpeedTime;

        public override void Hook()
        {
            ModHooks.HeroUpdateHook += SpeedUpTime;
        }

        public static void SpeedUpTime()
        {
            if (SpeedTime.Instance.Equipped() && !SlowTime.Instance.Equipped())
            {
                float num3 = 1.40f;
                Time.timeScale = num3;
            }
            if (SpeedTime.Instance.Equipped() && SlowTime.Instance.Equipped())
            {
                float num3 = 1.00f;
                Time.timeScale = num3;
            }
            if (!SpeedTime.Instance.Equipped() && !SlowTime.Instance.Equipped())
            {
                float num3 = 1.00f;
                Time.timeScale = num3;
            }

        }
    }
}