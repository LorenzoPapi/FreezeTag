using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace Impostor.Api.Innersloth
{
   public class VentTypes : IEnumerable
   {
        public static readonly Vent SkeldAdmin = new (VentLocation.SkeldAdmin, new Vector2(2.54f, -9.6f));
        public static readonly Vent SkeldRightHallway = new (VentLocation.SkeldRightHallway, new Vector2(9.47f, -6.04f));
        public static readonly Vent SkeldCafeteria = new (VentLocation.SkeldCafeteria, new Vector2(4.22f, 0.01f));
        public static readonly Vent SkeldElectrical = new (VentLocation.SkeldElectrical, new Vector2(-9.78f, -7.69f));
        public static readonly Vent SkeldUpperEngine = new (VentLocation.SkeldUpperEngine, new Vector2(-15.28f, -2.79f));
        public static readonly Vent SkeldSecurity = new (VentLocation.SkeldSecurity, new Vector2(-12.53f, -6.59f));
        public static readonly Vent SkeldMedbay = new (VentLocation.SkeldMedbay, new Vector2(-10.54f, -3.85f));
        public static readonly Vent SkeldWeapons = new (VentLocation.SkeldWeapons, new Vector2(0, 0));
        public static readonly Vent SkeldLowerReactor = new (VentLocation.SkeldLowerReactor, new Vector2(-20.73f, -6.66f));
        public static readonly Vent SkeldLowerEngine = new (VentLocation.SkeldLowerEngine, new Vector2(-15.25f, -13.29f));
        public static readonly Vent SkeldShields = new (VentLocation.SkeldShields, new Vector2(9.51f, -14.06f));
        public static readonly Vent SkeldUpperReactor = new (VentLocation.SkeldUpperReactor, new Vector2(-21.88f, -2.69f));
        public static readonly Vent SkeldUpperNavigation = new (VentLocation.SkeldUpperNavigation, new Vector2(16.01f, -2.89f));
        public static readonly Vent SkeldLowerNavigation = new (VentLocation.SkeldLowerNavigation, new Vector2(16.01f, -6.02f));

        public IEnumerator GetEnumerator()
        {
            foreach (FieldInfo prop in typeof(VentTypes).GetFields())
            {
                yield return prop.GetValue(this);
            }
        }
    }
}
