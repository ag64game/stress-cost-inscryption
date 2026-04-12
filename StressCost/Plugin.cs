using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DiskCardGame;
using EasyFeedback.APIs;
using GBC;
using HarmonyLib;
using InscryptionAPI.Ascension;
using InscryptionAPI.Boons;
using InscryptionAPI.Card;
using InscryptionAPI.Card.CostProperties;
using InscryptionAPI.Encounters;
using InscryptionAPI.Helpers;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Nodes;
using InscryptionAPI.PixelCard;
using InscryptionAPI.Regions;
using InscryptionAPI.Sound;
using InscryptionAPI.Triggers;
using InscryptionCommunityPatch.Card;
using InscryptionCommunityPatch.PixelTutor;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using Sirenix.Serialization.Utilities;
using Steamworks;
using StressCost.Cost;
using StressCost.Sigils;
using StressCost.Sigils.VariableStats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using UnityEngine.UI;
using static InscryptionAPI.CardCosts.CardCostManager;
using static System.Net.Mime.MediaTypeNames;


namespace StressCost
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CostmaniaPlugin : BaseUnityPlugin
    {
        public const string GUID = "aga.costmania";
        public const string NAME = "CostMania";
        private const string VERSION = "0.0.6.7";

        public static string Directory;
        internal static ManualLogSource Log;

        Harmony harmony = new Harmony(GUID);

        public static readonly string[] NEW_TEMPLES = ["Alchemy", "Stress", "Space", "Valor"];

        internal static ConfigEntry<bool> config3DAlchemy;
        internal static ConfigEntry<bool> config3DStress;
        internal static ConfigEntry<bool> config3DStardust;
        internal static ConfigEntry<bool> config3DValor;

        private void Awake()
        {
            Log = base.Logger;
            Directory = base.Info.Location.Replace("Costmania.dll", "");
            harmony.PatchAll();

            AddCosts();
            AddSigils();

            harmony.PatchAll(typeof(CostmaniaPlugin));
            harmony.PatchAll(typeof(Patches.AbilityPatches));
            harmony.PatchAll(typeof(Patches.CollectionUIPatches));
            harmony.PatchAll(typeof(Patches.CostGraphicPatches));
            harmony.PatchAll(typeof(Patches.CostPatches));
            harmony.PatchAll(typeof(Patches.PackPatches));

            Patches.PackPatches.SetupStarterDecks();
            
            config3DAlchemy = base.Config.Bind<bool>("Alchemy in 3D", "Active", false, "Whether Alchemy dies should be rolled in the 3D acts, won't display them though");
            config3DStress = base.Config.Bind<bool>("Stress in 3D", "Active", false, "Whether the Stress Counter should be active in the 3D acts, won't display it though");
            config3DStardust = base.Config.Bind<bool>("Stardust in 3D", "Active", false, "Whether the Stardust Counter should be active in the 3D acts, won't display it though");
            config3DValor = base.Config.Bind<bool>("Valor in 3D", "Active", false, "Whether to do the Promotion Phase or not in the 3D acts, though it won't display the Max Valor Counter");
        }

        private void Update()
        {
            Patches.CostPatches.Update();
            Patches.CostGraphicPatches.Update();
            Patches.PackPatches.Update();
        }  

        public static void AddCosts()
        {
            FullCardCost stressCost = Register(GUID, "StressCost", typeof(Cost.StressCost), Cost.StressCost.Texture_3D, Cost.StressCost.Texture_Pixel);
            stressCost.SetCostTier(Cost.CostTier.CostTierS);
            stressCost.ResourceType = (ResourceType)42;
            stressCost.SetCanBePlayedByTurn2WithHand(Cost.FairHandStress.CanBePlayed);

            FullCardCost valorCost = Register(GUID, "ValorCost", typeof(Cost.ValorCost), Cost.ValorCost.Texture_3D, Cost.ValorCost.Texture_Pixel);
            valorCost.SetCostTier(Cost.CostTier.CostTierV);
            valorCost.ResourceType = (ResourceType)42;
            valorCost.SetCanBePlayedByTurn2WithHand(Cost.FairHandValor.CanBePlayed);

            FullCardCost fleshCost = Register(GUID, "FleshCost", typeof(Cost.FleshCost), Cost.FleshCost.Texture_3D, Cost.FleshCost.Texture_Pixel);
            fleshCost.SetCostTier(Cost.CostTier.CostTierA);
            fleshCost.ResourceType = (ResourceType)42;
            fleshCost.SetCanBePlayedByTurn2WithHand(Cost.FairHandFlesh.CanBePlayed);

            FullCardCost metalCost = Register(GUID, "MetalCost", typeof(Cost.MetalCost), Cost.MetalCost.Texture_3D, Cost.MetalCost.Texture_Pixel);
            metalCost.SetCostTier(Cost.CostTier.CostTierA);
            metalCost.ResourceType = (ResourceType)42;
            metalCost.SetCanBePlayedByTurn2WithHand(Cost.FairHandMetal.CanBePlayed);

            FullCardCost elixirCost = Register(GUID, "ElixirCost", typeof(Cost.ElixirCost), Cost.ElixirCost.Texture_3D, Cost.ElixirCost.Texture_Pixel);
            elixirCost.SetCostTier(Cost.CostTier.CostTierA);
            elixirCost.ResourceType = (ResourceType)42;
            elixirCost.SetCanBePlayedByTurn2WithHand(Cost.FairHandElixir.CanBePlayed);

            FullCardCost stardustCost = Register(GUID, "StardustCost", typeof(Cost.StardustCost), Cost.StardustCost.Texture_3D, Cost.StardustCost.Texture_Pixel);
            stardustCost.SetCostTier(Cost.CostTier.CostTierF);
            stardustCost.ResourceType = (ResourceType)42;
            stardustCost.SetCanBePlayedByTurn2WithHand(Cost.FairHandStardust.CanBePlayed);
        }

        public static void AddSigils()
        {
            AbilRelaxant.AddRelaxant();
            AbilAffection.AddAffection();
            AbilEnrage.AddEnrage();
            AbilLiftoff.AddLiftoff();
            AbilGemAbsorber.AddGemAbsorber();
            AbilFrontliner.AddFrontliner();
            AbilBloodGuzzler.AddBloodGuzzler();
            AbilIronclad.AddIronclad();
            AbilFearmonger.AddFearmonger();
            AbilFirstStrike.AddFirstStrike();
            AbilWatchman.AddWatchman();
            AbilEldritchPower.AddEldritchPower();
            AbilSigilEater.AddSigilEater();
            AbilStarbringer.AddStrbringer();
            AbilHealingAura.AddHealingAura();
            AbilWarper.AddWarper();
            AbilShatteringStardust.AddShatteringStardust();
            AbilArmyBuilder.AddArmyBuilder();
            AbilRandomAbility.AddRandomAbility();
            AbilAfterimage.AddAfterimage();
            AbilRentrance.AddRentrance();

            VariablestatMightierPen.AddMightierPen();
            VariablestatDeathToll.AddDeathToll();
        }

    }
}