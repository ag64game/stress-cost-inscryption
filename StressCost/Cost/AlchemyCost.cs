using DiskCardGame;
using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Card;
using InscryptionAPI.CardCosts;
using InscryptionAPI.Helpers;
using InscryptionCommunityPatch.Card;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static InscryptionAPI.CardCosts.CardCostManager;
using Press = UnityEngine.UI.Button;

namespace StressCost.Cost
{
    public enum AlchemyValue
    {
        Empty,
        Flesh,
        Metal,
        Elixir,
    }

    internal class AlchemyDice : MonoBehaviour
    {
        private AlchemyValue __value = AlchemyValue.Empty;
        public AlchemyValue value { get; set; }

        public bool locked = false;
        private SpriteRenderer renderer;
        public static int texWidth = 0, texHeight = 0;

        public Vector3 pivot = new Vector3(0f, 0f, 0.1f);

        private void Awake()
        {
            renderer = gameObject.GetComponent<SpriteRenderer>();
            //gameObject.AddComponent<CardAnimationController>();
        }

        private void Update()
        {
            Texture2D texture = TextureHelper.GetImageAsTexture($"displaycost_{value.ToString().ToLower()}_alchemy_die.png", typeof(CostmaniaPlugin).Assembly);
            texWidth = texture.width;
            texHeight = texture.height;

            renderer.sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texWidth, texHeight),
                    pivot
                    );
            renderer.sprite.name = "dice_sprite";

            if (value == AlchemyValue.Empty) locked = false;

            float lockedColor = 0.5f * (locked ? 1 : 2);
            renderer.color = new Color(lockedColor, lockedColor, lockedColor);
        }

        public void OnMouseDown()
        {
            if (value != AlchemyValue.Empty) locked = !locked;
        }
    }

    public class AlchemyCounter : MonoBehaviour
    {
        private AlchemyDice[] dies = new AlchemyDice[10];
        public int fleshCount
        {
            get
            {
                return dies.Where(die => die.value == AlchemyValue.Flesh).Count();
            }
        }
        public int metalCount
        {
            get
            {
                return dies.Where(die => die.value == AlchemyValue.Metal).Count();
            }
        }
        public int elixirCount
        {
            get
            {
                return dies.Where(die => die.value == AlchemyValue.Elixir).Count();
            }
        }
        int awakeCount
        {
            get
            {
                return dies.Where(die => die.value != AlchemyValue.Empty).Count();
            }
        }

        private float size = 0.12f;

        private void Awake()
        {
            float x = -2.125f, y = 0.51f;

            for (int i = 0; i < dies.Length; i++)
            {
                var position  = new Vector3(x, y, 0.1f);
                GameObject dieObj = Instantiate(PixelResourcesManager.Instance.gameObject.transform.Find("Blood").Find("Blood").gameObject);
                dieObj.name = $"AlchemyDice {i + 1}";

                BoxCollider2D col = dieObj.AddComponent<BoxCollider2D>();
                col.size = new Vector2(size, size);
                dieObj.transform.position = position;

                dies[i] = dieObj.AddComponent<AlchemyDice>();
                dies[i].pivot = new Vector3(0.36f, 0.35f, 0f);
                dies[i].gameObject.transform.SetParent(transform, false);


                if ((i + 1) % 2 == 0)
                {
                    x -= size;
                    y -= size;
                }
                else x += size;
            }
        }

        public void AddDies(int amount = 1, AlchemyValue? specific = null)
        {
            try
            {
                for (int i = Array.IndexOf(dies, dies.Where(die => die.value == AlchemyValue.Empty).First()); amount > 0 && i < 10; i++)
                {
                    RollDice(i, specific);
                    amount--;
                }
            }
            catch { }
        }

        public void RollDies(int? index = null, AlchemyValue? specific = null)
        {
            if (index != null) RollDice(index.Value, specific);
            else for(int i = 0; i < awakeCount; i++) RollDice(i, specific);
        }

        private void RollDice(int index, AlchemyValue? specific = null)
        {
            if (!dies[index].locked)
            {
                if (specific != null) dies[index].value = specific.Value;
                else
                {
                    dies[index].value = (AlchemyValue)UnityEngine.Random.Range(1, 4);
                }
                //dies[index].anim.StrongNegationEffect();
            }
        }

        public bool PayIfPossible(AlchemyValue type, int amount)
        {
            switch (type)
            {
                case AlchemyValue.Flesh:
                    if (fleshCount < amount) return false;
                    break;
                case AlchemyValue.Metal:
                    if (metalCount < amount) return false;
                    break;
                case AlchemyValue.Elixir:
                    if (elixirCount < amount) return false;
                    break;
                default:
                    throw new ArgumentException("Cannot pay with 'Empty' Alchemy");
            }

            DepleteDies(type, amount);
            return true;
        }

        private void DepleteDies(AlchemyValue toDrop, int amount)
        {
            for (int i = 0; i < 10 && amount > 0; i++)
            {  
                if (dies[i].value == toDrop && amount > 0)
                {
                    dies[i].value = AlchemyValue.Empty;
                    for (int j = i; j < 9 && dies[j + 1].value != AlchemyValue.Empty; j++)
                    {
                        dies[j].value = dies[j + 1].value;
                        dies[j].locked = dies[j + 1].locked;
                        dies[j + 1].value = AlchemyValue.Empty;
                    }
                    amount--;
                    i--;
                }
            }

            //if (amount > 0) dies[j].value = AlchemyValue.Empty;
        }
    }



    internal class FleshCost : CustomCardCost
    {
        public override string CostName => "FleshCost";

        public override bool CostSatisfied(int cardCost, PlayableCard card)
        {
            return cardCost <= CostmaniaPlugin.disAlchemyCounter.fleshCount;
        }

        public override string CostUnsatisfiedHint(int cardCost, PlayableCard card)
        {
            if (SaveManager.SaveFile.IsPart2) return $"You do not have the right amount of [c:green]Alchemy Dies[c:] to play this card. [c:green]Alchemy Dies[c:] are generated over time.";
            else
            {
                var choice1 = $"[c:green]{card.Info.DisplayedNameLocalized}[c:] can only be played once you've accumulated enough nessecary [c:green]dies[c:].";
                var choice2 = $"Your [c:green]Alchemy Dies[c:] are of insufficient quantities.";
                var choice3 = $"Don't be a fool, you need [c:green]{cardCost} Flesh Dies[c:] to play [c:green]{card.Info.DisplayedNameLocalized}[c:]";

                List<String> strings = new List<String>();
                strings.Add(choice1);
                strings.Add(choice2);
                strings.Add(choice3);

                return strings[UnityEngine.Random.Range(0, strings.Count)];
            }
        }

        public static Texture2D Texture_3D(int cardCost, CardInfo info, PlayableCard card)
        {
            return TextureHelper.GetImageAsTexture($"FleshCost_{cardCost}.png", typeof(CostmaniaPlugin).Assembly);
        }

        public static Texture2D Texture_Pixel(int cardCost, CardInfo info, PlayableCard card)
        {
            // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
            return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("pixelcost_flesh.png", typeof(CostmaniaPlugin).Assembly));
        }
    }
    internal class MetalCost : CustomCardCost
    {
        public override string CostName => "MetalCost";

        public override bool CostSatisfied(int cardCost, PlayableCard card)
        {
            return cardCost <= CostmaniaPlugin.disAlchemyCounter.metalCount;
        }

        public override string CostUnsatisfiedHint(int cardCost, PlayableCard card)
        {
            if (SaveManager.SaveFile.IsPart2) return $"You do not have the right amount of [c:green]Alchemy Dies[c:] to play this card. [c:green]Alchemy Dies[c:] are generated over time.";
            else
            {
                var choice1 = $"[c:green]{card.Info.DisplayedNameLocalized}[c:] can only be played once you've accumulated enough nessecary [c:green]dies[c:].";
                var choice2 = $"Your [c:green]Alchemy Dies[c:] are of insufficient quantities.";
                var choice3 = $"Don't be a fool, you need [c:green]{cardCost} Metal Dies[c:] to play [c:green]{card.Info.DisplayedNameLocalized}[c:]";

                List<String> strings = new List<String>();
                strings.Add(choice1);
                strings.Add(choice2);
                strings.Add(choice3);

                return strings[UnityEngine.Random.Range(0, strings.Count)];
            }
        }

        public static Texture2D Texture_3D(int cardCost, CardInfo info, PlayableCard card)
        {
            return TextureHelper.GetImageAsTexture($"MetalCost_{cardCost}.png", typeof(CostmaniaPlugin).Assembly);
        }

        public static Texture2D Texture_Pixel(int cardCost, CardInfo info, PlayableCard card)
        {
            // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
            return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("pixelcost_metal.png", typeof(CostmaniaPlugin).Assembly));
        }
    }
    internal class ElixirCost : CustomCardCost
    {
        public override string CostName => "ElixirCost";

        public override bool CostSatisfied(int cardCost, PlayableCard card)
        {
            return cardCost <= CostmaniaPlugin.disAlchemyCounter.elixirCount;
        }

        public override string CostUnsatisfiedHint(int cardCost, PlayableCard card)
        {
            if (SaveManager.SaveFile.IsPart2) return $"You do not have the right amount of [c:green]Alchemy Dies[c:] to play this card. [c:green]Alchemy Dies[c:] are generated over time.";
            else
            {
                var choice1 = $"[c:green]{card.Info.DisplayedNameLocalized}[c:] can only be played once you've accumulated enough nessecary [c:green]dies[c:].";
                var choice2 = $"Your [c:green]Alchemy Dies[c:] are of insufficient quantities.";
                var choice3 = $"Don't be a fool, you need [c:green]{cardCost} Elixir Dies[c:] to play [c:green]{card.Info.DisplayedNameLocalized}[c:]";

                List<String> strings = new List<String>();
                strings.Add(choice1);
                strings.Add(choice2);
                strings.Add(choice3);

                return strings[UnityEngine.Random.Range(0, strings.Count)];
            }
        }

        public static Texture2D Texture_3D(int cardCost, CardInfo info, PlayableCard card)
        {
            return TextureHelper.GetImageAsTexture($"ElixirCost_{cardCost}.png", typeof(CostmaniaPlugin).Assembly);
        }

        public static Texture2D Texture_Pixel(int cardCost, CardInfo info, PlayableCard card)
        {
            // if you want the API to handle adding stack numbers, you can instead provide a 7x8 texture like so:
            return Part2CardCostRender.CombineIconAndCount(cardCost, TextureHelper.GetImageAsTexture("pixelcost_elixir.png", typeof(CostmaniaPlugin).Assembly));
        }
    }
}
