using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EvoVI.Database;

namespace EvoVILIib_Tests
{
    [TestClass]
    public class DatabaseTests
    {
        [TestMethod]
        public void DatabaseBuildTest()
        {
            GameMeta.CurrentGame = GameMeta.SupportedGame.EVOCHRON_MERCENARY;
            DatabasePropertyTest();

            GameMeta.CurrentGame = GameMeta.SupportedGame.EVOCHRON_LEGACY;
            DatabasePropertyTest();

            GameMeta.CurrentGame = GameMeta.SupportedGame.NONE;
            DatabasePropertyTest();
        }

        [TestMethod]
        public void DatabasePropertyTest()
        {
            SaveDataReader.BuildDatabase();
            object obj;

            obj = EnvironmentalData.InboundMissileAlert;
            obj = EnvironmentalData.WaypointCoordinates;
            obj = EnvironmentalData.WaypointSectorCoordinates;
            obj = EnvironmentalData.NavPointDistance;
            obj = EnvironmentalData.LocalSystemName;
            obj = EnvironmentalData.GravityLevel;

            obj = TargetShipData.CargoBay;
            obj = TargetShipData.CapitalShipTurret;
            obj = TargetShipData.ShieldLevel;
            obj = TargetShipData.Description;
            obj = TargetShipData.ThreatLevel;
            obj = TargetShipData.Range;
            obj = TargetShipData.EngineDamage;
            obj = TargetShipData.WeaponDamage;
            obj = TargetShipData.NavDamage;
            obj = TargetShipData.Faction;
            obj = TargetShipData.DamageLevel;
            obj = TargetShipData.Velocity;
            obj = TargetShipData.EngineClass;
            obj = TargetShipData.ResistorPacks;
            obj = TargetShipData.HullPlating;
            obj = TargetShipData.ModuleType;
            obj = TargetShipData.WingClass;

            obj = HudData.TotalHostilesOnRadar;
            obj = HudData.NavigationConsole;
            obj = HudData.BuildConsole;
            obj = HudData.InventoryConsole;
            obj = HudData.TradeConsole;
            obj = HudData.Hud;
            obj = HudData.TargetDisplay;

            obj = PlayerData.Name;
            obj = PlayerData.TotalKills;
            obj = PlayerData.TotalContracts;
            obj = PlayerData.SkillAndProficiencyRating;
            obj = PlayerData.MilitaryRating;
            obj = PlayerData.Cash;

            obj = PlayerShipData.Fuel;
            obj = PlayerShipData.EnergyLevel;
            obj = PlayerShipData.EngineDamage;
            obj = PlayerShipData.WeaponDamage;
            obj = PlayerShipData.NavDamage;
            obj = PlayerShipData.ParticleCannon;
            obj = PlayerShipData.BeamCannon;
            obj = PlayerShipData.ShipType;
            obj = PlayerShipData.EngineClass;
            obj = PlayerShipData.ShieldClass;
            obj = PlayerShipData.CargoCapacity;
            obj = PlayerShipData.WingClass;
            obj = PlayerShipData.ThrusterClass;
            obj = PlayerShipData.CrewLimit;
            obj = PlayerShipData.EquipmentLimit;
            obj = PlayerShipData.CountermeasureLimit;
            obj = PlayerShipData.HardpointLimit;
            obj = PlayerShipData.ArmorLimit;
            obj = PlayerShipData.ParticleCannonRange;
            obj = PlayerShipData.MissileRange;
            obj = PlayerShipData.TargetedSubsystem;
            obj = PlayerShipData.CounterMeasures;
            obj = PlayerShipData.IDSMultiplier;
            obj = PlayerShipData.Velocity;
            obj = PlayerShipData.SetVelocity;
            obj = PlayerShipData.Altitude;
            obj = PlayerShipData.HeatSignatureLevel;
            obj = PlayerShipData.TotalVelocity;
            obj = PlayerShipData.Heading;
            obj = PlayerShipData.Pitch;
            obj = PlayerShipData.CargoBay;
            obj = PlayerShipData.Position;
            obj = PlayerShipData.SectorPosition;
            obj = PlayerShipData.ShieldLevel;
            obj = PlayerShipData.SecWeapon;
            obj = PlayerShipData.EquipmentSlot;
            obj = PlayerShipData.Heat;
            obj = PlayerShipData.Mtds;
            obj = PlayerShipData.MissileLock;
            obj = PlayerShipData.ShieldBias;
            obj = PlayerShipData.WeaponBias;
            obj = PlayerShipData.Ids;
            obj = PlayerShipData.Afterburner;
            obj = PlayerShipData.Autopilot;
            obj = PlayerShipData.TractorBeam;
        }
    }
}
